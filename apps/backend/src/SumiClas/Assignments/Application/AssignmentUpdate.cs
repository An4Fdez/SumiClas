using MediatR;
using SumiClas.Assignments.Domain;
using SumiClas.Classes.Application;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Subjects.Application;

namespace SumiClas.Assignments.Application;

public static class AssignmentUpdate
{
    /// <summary>
    /// The author changes the subject, the title, the description and the due date. The comments stay, and the done
    /// marks of the whole class are dropped. Whether it is official or a proposal stays as it was created.
    /// </summary>
    public sealed record Command(
        Guid Id,
        Guid SubjectId,
        string Title,
        string? Description,
        DateOnly DueDate) : IRequest, ICommand;

    public sealed class Update(IAssignmentRepository repository, IQueryBus queryBus, IClock clock, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            Guid current = userProvider.GetCurrentUserId();

            Assignment assignment = await repository.GetByIdAsync(command.Id, ct)
                ?? throw new AssignmentExceptions.NotFound();

            if (assignment.AuthorId != current)
            {
                throw new AssignmentExceptions.ChangeForbidden();
            }

            Assignment.ValidateContent(command.SubjectId, command.Title);

            ClassResponse schoolClass = await queryBus.Send(new ClassFinderById.Query(assignment.ClassId), ct);

            await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.SchoolYearId), ct);

            SubjectResponse subject = await queryBus.Send(new SubjectFinderById.Query(command.SubjectId), ct);
            if (subject.ClassId != assignment.ClassId)
            {
                throw new AssignmentExceptions.SubjectNotInClass();
            }

            assignment.Update(command.SubjectId, command.Title, command.Description, command.DueDate, clock);
        }
    }

    public sealed class Handler(Update update) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => update.ExecuteAsync(command, ct);
    }
}
