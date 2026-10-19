using MediatR;
using SumiClas.Classes.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.Classes.Application;

public static class ClassRename
{
    public sealed record Command(Guid ClassId, string Name) : IRequest<Result>, ICommand;

    public sealed class Rename(IClassRepository repository, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var schoolClass = await repository.GetByIdAsync(command.ClassId, ct);
            if (schoolClass is null)
            {
                return Result.Failure(ClassErrors.NotFound);
            }

            var current = userProvider.GetCurrentUserId();
            if (current.IsFailure)
            {
                return Result.Failure(current.Error!);
            }

            var representative = await queryBus.Send(new UserClassRepresentativeEnsure.Query(current.Value, command.ClassId), ct);
            if (representative.IsFailure)
            {
                return representative;
            }

            var schoolYear = await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.SchoolYearId), ct);
            if (schoolYear.IsFailure)
            {
                return schoolYear;
            }

            return schoolClass.Rename(command.Name);
        }
    }

    public sealed class Handler(Rename rename) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => rename.ExecuteAsync(command, ct);
    }
}
