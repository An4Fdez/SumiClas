using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Students.Domain;
using SumiClas.Users.Application;

namespace SumiClas.Students.Application;

public static class StudentGuardianAdd
{
    public sealed record Command(Guid StudentId, Guid GuardianId, string Role) : IRequest<Result>, ICommand;

    public sealed class Add(IStudentRepository students, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var current = userProvider.GetCurrentUserId();
            if (current.IsFailure)
            {
                return Result.Failure(current.Error!);
            }

            if (!Enum.TryParse<GuardianRole>(command.Role, ignoreCase: true, out var role) || !Enum.IsDefined(role))
            {
                return Result.Failure(StudentErrors.InvalidGuardianRole);
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

            var guardianUser = await queryBus.Send(new UserFinderById.Query(command.GuardianId), ct);
            if (guardianUser.IsFailure)
            {
                return Result.Failure(guardianUser.Error!);
            }

            return student.AddGuardian(command.GuardianId, role);
        }
    }

    public sealed class Handler(Add add) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => add.ExecuteAsync(command, ct);
    }
}
