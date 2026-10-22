using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Students.Domain;
using SumiClas.Users.Application;

namespace SumiClas.Students.Application;

public static class StudentGuardianAdd
{
    public sealed record Command(Guid StudentId, Guid GuardianId, string Role) : IRequest, ICommand;

    public sealed class Add(IStudentRepository repository, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var current = userProvider.GetCurrentUserId();

            if (!Enum.TryParse<GuardianRole>(command.Role, ignoreCase: true, out var role) || !Enum.IsDefined(role))
            {
                throw new StudentExceptions.InvalidGuardianRole();
            }

            var student = await repository.GetByIdAsync(command.StudentId, ct)
                ?? throw new StudentExceptions.NotFound();

            if (student.Guardians.All(guardian => guardian.GuardianId != current))
            {
                throw new StudentExceptions.GuardianChangeForbidden();
            }

            await queryBus.Send(new UserFinderById.Query(command.GuardianId), ct);

            student.AddGuardian(command.GuardianId, role);
        }
    }

    public sealed class Handler(Add add) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => add.ExecuteAsync(command, ct);
    }
}
