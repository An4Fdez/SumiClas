using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Students.Domain;

namespace SumiClas.Students.Application;

public static class StudentGuardianRemove
{
    public sealed record Command(Guid StudentId, Guid GuardianId) : IRequest, ICommand;

    public sealed class Remove(IStudentRepository repository, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var current = userProvider.GetCurrentUserId();

            var student = await repository.GetByIdAsync(command.StudentId, ct)
                ?? throw new StudentExceptions.NotFound();

            if (student.Guardians.All(guardian => guardian.GuardianId != current))
            {
                throw new StudentExceptions.GuardianChangeForbidden();
            }

            student.RemoveGuardian(command.GuardianId);
        }
    }

    public sealed class Handler(Remove remove) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => remove.ExecuteAsync(command, ct);
    }
}
