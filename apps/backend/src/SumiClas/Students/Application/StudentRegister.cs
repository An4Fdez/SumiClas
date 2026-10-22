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
        string FirstGuardianRole) : IRequest, ICommand;

    public sealed class Register(IStudentRepository repository, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            Guid guardianId = userProvider.GetCurrentUserId();

            if (!Enum.TryParse<GuardianRole>(command.FirstGuardianRole, ignoreCase: true, out GuardianRole role)
                || !Enum.IsDefined(role))
            {
                throw new StudentExceptions.InvalidGuardianRole();
            }

            StudentName name = StudentName.Create(command.FirstName, command.FirstSurname, command.SecondSurname);

            Student student = Student.Register(command.Id, name, guardianId, role);

            await queryBus.Send(new UserFinderById.Query(guardianId), ct);

            await repository.AddAsync(student, ct);
        }
    }

    public sealed class Handler(Register register) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => register.ExecuteAsync(command, ct);
    }
}
