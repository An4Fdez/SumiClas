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
    public sealed record Command(Guid SchoolYearId) : IRequest<Result>, ICommand;

    public sealed class Archive(ISchoolYearRepository repository, IQueryBus queryBus, IClock clock, IUserProvider userProvider) : IUseCase
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var schoolYear = await repository.GetByIdAsync(command.SchoolYearId, ct);
            if (schoolYear is null)
            {
                return Result.Failure(SchoolYearErrors.NotFound);
            }

            var current = userProvider.GetCurrentUserId();
            if (current.IsFailure)
            {
                return Result.Failure(current.Error!);
            }

            var classes = await queryBus.Send(new ClassFinderBySchoolYear.Query(command.SchoolYearId), ct);
            foreach (var schoolClass in classes)
            {
                var representative = await queryBus.Send(
                    new UserClassRepresentativeEnsure.Query(current.Value, schoolClass.Id), ct);
                if (representative.IsSuccess)
                {
                    return schoolYear.Archive(clock);
                }
            }

            return Result.Failure(UserErrors.ClassRepresentativeForbidden);
        }
    }

    public sealed class Handler(Archive archive) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => archive.ExecuteAsync(command, ct);
    }
}
