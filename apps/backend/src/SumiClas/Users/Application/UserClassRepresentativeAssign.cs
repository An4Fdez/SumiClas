using MediatR;
using SumiClas.Classes.Application;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Users.Domain;

namespace SumiClas.Users.Application;

public static class UserClassRepresentativeAssign
{
    /// <summary>Only a current delegate of the class (the current user) can give the role to <paramref name="UserId"/>.</summary>
    public sealed record Command(Guid UserId, Guid ClassId) : IRequest, ICommand;

    public sealed class Assign(
        IUserRepository repository,
        IQueryBus queryBus,
        IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            Guid actingUserId = userProvider.GetCurrentUserId();

            ClassResponse schoolClass = await queryBus.Send(new ClassFinderById.Query(command.ClassId), ct);

            await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.SchoolYearId), ct);

            IReadOnlyCollection<User> actingRepresentative = await repository.SearchAsync(
                new UserSpecification(id: actingUserId, classId: command.ClassId, classRole: ClassRoleType.ClassRepresentative), ct);
            if (actingRepresentative.Count == 0)
            {
                throw new UserExceptions.AssignForbidden();
            }

            User user = await repository.GetByIdAsync(command.UserId, ct)
                ?? throw new UserExceptions.NotFound();

            IReadOnlyCollection<User> representatives = await repository.SearchAsync(
                new UserSpecification(classId: command.ClassId, classRole: ClassRoleType.ClassRepresentative), ct);

            user.AssignAsClassRepresentative(command.ClassId, representatives.Count);
        }
    }

    public sealed class Handler(Assign assign) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => assign.ExecuteAsync(command, ct);
    }
}
