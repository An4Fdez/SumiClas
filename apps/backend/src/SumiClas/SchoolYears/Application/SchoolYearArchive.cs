using MediatR;
using SumiClas.Classes.Application;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;
    
namespace SumiClas.SchoolYears.Application;

public static class SchoolYearArchive
{
    /// <summary>Only a delegate of one of the classes of the school year may archive it.</summary>
    public sealed record Command(Guid SchoolYearId) : IRequest, ICommand;

    public sealed class Archive(ISchoolYearRepository repository, IQueryBus queryBus, IClock clock, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            Guid current = userProvider.GetCurrentUserId();

            SchoolYear schoolYear = await repository.GetByIdAsync(command.SchoolYearId, ct)
                ?? throw new SchoolYearExceptions.NotFound();

            IReadOnlyCollection<ClassResponse> classes = await queryBus.Send(new ClassFinderBySchoolYear.Query(command.SchoolYearId), ct);
            Guid[] classIds = classes.Select(schoolClass => schoolClass.Id).ToArray();
            await queryBus.Send(new UserAnyClassRepresentativeEnsure.Query(current, classIds), ct);

            schoolYear.Archive(clock);
        }
    }

    public sealed class Handler(Archive archive) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => archive.ExecuteAsync(command, ct);
    }
}
