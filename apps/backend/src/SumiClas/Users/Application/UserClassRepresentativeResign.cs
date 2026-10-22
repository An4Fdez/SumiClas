using MediatR;
using SumiClas.Classes.Application;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Users.Domain;

namespace SumiClas.Users.Application;

public static class UserClassRepresentativeResign
{
    public sealed record Command(Guid ClassId) : IRequest, ICommand;

    public sealed class Resign(IUserRepository repository, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            Guid userId = userProvider.GetCurrentUserId();

            ClassResponse schoolClass = await queryBus.Send(new ClassFinderById.Query(command.ClassId), ct);

            await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.SchoolYearId), ct);

            User user = await repository.GetByIdAsync(userId, ct)
                ?? throw new UserExceptions.NotFound();

            IReadOnlyCollection<User> representatives = await repository.SearchAsync(
                new UserSpecification(classId: command.ClassId, classRole: ClassRoleType.ClassRepresentative), ct);
            if (representatives.Count == 1 && representatives.Single().Id == user.Id)
            {
                throw new UserExceptions.LastClassRepresentativeCannotResign();
            }

            user.ResignAsClassRepresentative(command.ClassId);
        }
    }

    public sealed class Handler(Resign resign) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => resign.ExecuteAsync(command, ct);
    }
}
