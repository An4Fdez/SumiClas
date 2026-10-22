using MediatR;
using SumiClas.Classes.Application;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

namespace SumiClas.SchoolYears.Application;

public static class SchoolYearArchive
{
    /// <summary>Only a delegate of one of the classes of the school year may archive it.</summary>
    public sealed record Command(Guid SchoolYearId) : IRequest, ICommand;

    public sealed class Archive(ISchoolYearRepository repository, IQueryBus queryBus, IClock clock, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            SchoolYear schoolYear = await repository.GetByIdAsync(command.SchoolYearId, ct)
                ?? throw new SchoolYearExceptions.NotFound();

            Guid current = userProvider.GetCurrentUserId();

            IReadOnlyCollection<ClassResponse> classes = await queryBus.Send(new ClassFinderBySchoolYear.Query(command.SchoolYearId), ct);
            foreach (ClassResponse schoolClass in classes)
            {
                bool isRepresentative = await queryBus.Send(new UserIsClassRepresentative.Query(current, schoolClass.Id), ct);
                if (isRepresentative)
                {
                    schoolYear.Archive(clock);
                    return;
                }
            }

            throw new UserExceptions.ClassRepresentativeForbidden();
        }
    }

    public sealed class Handler(Archive archive) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => archive.ExecuteAsync(command, ct);
    }
}
