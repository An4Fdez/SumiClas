using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Students.Domain;
using SumiClas.Users.Application;

namespace SumiClas.Students.Application;

public static class StudentRegister
{
    public sealed record Command(
        Guid Id,
        string FirstName,
        string FirstSurname,
        string? SecondSurname,
        string FirstGuardianRole) : IRequest<Result>, ICommand;

    public sealed class Register(IStudentRepository students, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var current = userProvider.GetCurrentUserId();
            if (current.IsFailure)
            {
                return Result.Failure(current.Error!);
            }

            var guardianId = current.Value;
            if (!Enum.TryParse<GuardianRole>(command.FirstGuardianRole, ignoreCase: true, out var role)
                || !Enum.IsDefined(role))
            {
                return Result.Failure(StudentErrors.InvalidGuardianRole);
            }

            var name = StudentName.Create(command.FirstName, command.FirstSurname, command.SecondSurname);
            if (name.IsFailure)
            {
                return Result.Failure(name.Error!);
            }

            var registration = Student.Register(command.Id, name.Value, guardianId, role);
            if (registration.IsFailure)
            {
                return Result.Failure(registration.Error!);
            }

            var guardian = await queryBus.Send(new UserFinderById.Query(guardianId), ct);
            if (guardian.IsFailure)
            {
                return Result.Failure(guardian.Error!);
            }

            await students.AddAsync(registration.Value, ct);

            return Result.Success();
        }
    }

    public sealed class Handler(Register register) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => register.ExecuteAsync(command, ct);
    }
}
