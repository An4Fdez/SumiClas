using MediatR;
using SumiClas.Classes.Application;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Domain;

namespace SumiClas.Users.Application;

public static class UserClassRepresentativeAssign
{
    /// <summary>Only a current delegate of the class (the current user) can give the role to <paramref name="UserId"/>.</summary>
    public sealed record Command(Guid UserId, Guid ClassId) : IRequest<Result>, ICommand;

    public sealed class UseCase(
        IUserRepository users,
        IQueryBus queryBus,
        IUserProvider userProvider)
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var current = userProvider.GetCurrentUserId();
            if (current.IsFailure)
            {
                return Result.Failure(current.Error!);
            }

            var actingUserId = current.Value;
            var schoolClass = await queryBus.Send(new ClassFinderById.Query(command.ClassId), ct);
            if (schoolClass.IsFailure)
            {
                return Result.Failure(schoolClass.Error!);
            }

            var schoolYear = await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.Value.SchoolYearId), ct);
            if (schoolYear.IsFailure)
            {
                return schoolYear;
            }

            var actingRepresentative = await users.SearchAsync(
                new UserSpecification(id: actingUserId, classId: command.ClassId, classRole: ClassRoleType.ClassRepresentative), ct);
            if (actingRepresentative.Count == 0)
            {
                return Result.Failure(UserErrors.AssignForbidden);
            }

            var user = await users.GetByIdAsync(command.UserId, ct);
            if (user is null)
            {
                return Result.Failure(UserErrors.NotFound);
            }

            var representatives = await users.SearchAsync(
                new UserSpecification(classId: command.ClassId, classRole: ClassRoleType.ClassRepresentative), ct);

            return user.AssignAsClassRepresentative(command.ClassId, representatives.Count);
        }
    }

    public sealed class Handler(UseCase useCase) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => useCase.ExecuteAsync(command, ct);
    }
}
