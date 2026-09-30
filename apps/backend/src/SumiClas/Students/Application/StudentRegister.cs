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
        Guid FirstGuardianId,
        string FirstGuardianRole) : IRequest<Result>, ICommand;

    public sealed class UseCase(IStudentRepository students, IQueryBus queryBus)
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
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

            var registration = Student.Register(command.Id, name.Value, command.FirstGuardianId, role);
            if (registration.IsFailure)
            {
                return Result.Failure(registration.Error!);
            }

            var guardian = await queryBus.Send(new UserFinderById.Query(command.FirstGuardianId), ct);
            if (guardian.IsFailure)
            {
                return Result.Failure(guardian.Error!);
            }

            await students.AddAsync(registration.Value, ct);

            return Result.Success();
        }
    }

    public sealed class Handler(UseCase useCase) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => useCase.ExecuteAsync(command, ct);
    }
}
