using MediatR;
using SumiClas.Classes.Application;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Domain;

namespace SumiClas.Users.Application;

public static class UserClassRepresentativeResign
{
    public sealed record Command(Guid ClassId) : IRequest<Result>, ICommand;

    public sealed class Resign(IUserRepository repository, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var current = userProvider.GetCurrentUserId();
            if (current.IsFailure)
            {
                return Result.Failure(current.Error!);
            }

            var userId = current.Value;
            var schoolClass = await queryBus.Send(new ClassFinderById.Query(command.ClassId), ct);

            await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.SchoolYearId), ct);

            var user = await repository.GetByIdAsync(userId, ct);
            if (user is null)
            {
                return Result.Failure(UserErrors.NotFound);
            }

            var representatives = await repository.SearchAsync(
                new UserSpecification(classId: command.ClassId, classRole: ClassRoleType.ClassRepresentative), ct);
            if (representatives.Count == 1 && representatives.Single().Id == user.Id)
            {
                return Result.Failure(UserErrors.LastClassRepresentativeCannotResign);
            }

            return user.ResignAsClassRepresentative(command.ClassId);
        }
    }

    public sealed class Handler(Resign resign) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => resign.ExecuteAsync(command, ct);
    }
}
