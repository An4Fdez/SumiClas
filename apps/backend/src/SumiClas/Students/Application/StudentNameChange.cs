using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Students.Domain;

namespace SumiClas.Students.Application;

public static class StudentNameChange
{
    public sealed record Command(Guid StudentId, string FirstName, string FirstSurname, string? SecondSurname)
        : IRequest, ICommand;

    /// <summary>Guardianship is checked before the name, so a non-guardian learns nothing about what is valid.</summary>
    public sealed class Change(IStudentRepository repository, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            Guid current = userProvider.GetCurrentUserId();

            Student student = await repository.GetByIdAsync(command.StudentId, ct)
                ?? throw new StudentExceptions.NotFound();

            if (student.Guardians.All(guardian => guardian.GuardianId != current))
            {
                throw new StudentExceptions.GuardianAccessForbidden();
            }

            student.Rename(StudentName.Create(command.FirstName, command.FirstSurname, command.SecondSurname));
        }
    }

    public sealed class Handler(Change change) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => change.ExecuteAsync(command, ct);
    }
}
