using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Students.Domain;

namespace SumiClas.Students.Application;

public static class StudentGuardianRemove
{
    public sealed record Command(Guid StudentId, Guid GuardianId) : IRequest<Result>, ICommand;

    public sealed class UseCase(IStudentRepository students, IUserProvider userProvider)
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var current = userProvider.GetCurrentUserId();
            if (current.IsFailure)
            {
                return Result.Failure(current.Error!);
            }

            var student = await students.GetByIdAsync(command.StudentId, ct);
            if (student is null)
            {
                return Result.Failure(StudentErrors.NotFound);
            }

            if (student.Guardians.All(guardian => guardian.GuardianId != current.Value))
            {
                return Result.Failure(StudentErrors.GuardianChangeForbidden);
            }

            return student.RemoveGuardian(command.GuardianId);
        }
    }

    public sealed class Handler(UseCase useCase) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => useCase.ExecuteAsync(command, ct);
    }
}
