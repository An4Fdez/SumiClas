using MediatR;
using SumiClas.Assignments.Domain;
using SumiClas.Classes.Application;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Subjects.Application;
using SumiClas.Users.Application;

namespace SumiClas.Assignments.Application;

public static class AssignmentCreate
{
    /// <summary>There is no IsProposal here on purpose: it is derived from the author's role in the class.</summary>
    public sealed record Command(
        Guid Id,
        Guid ClassId,
        Guid SubjectId,
        string Title,
        string? Description,
        DateOnly DueDate) : IRequest, ICommand;

    public sealed class Create(IAssignmentRepository repository, IQueryBus queryBus, IClock clock, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            Guid current = userProvider.GetCurrentUserId();

            Guid authorId = current;

            // IsProposal depends on a lookup, so the aggregate is built twice: once only to validate the
            // input before any IO (the provisional flag is discarded), and once with the real flag.
            Assignment.Create(
                command.Id, command.ClassId, command.SubjectId, command.Title, command.Description,
                command.DueDate, authorId, isProposal: true, clock);

            UserResponse author = await queryBus.Send(new UserFinderById.Query(authorId), ct);

            ClassResponse schoolClass = await queryBus.Send(new ClassFinderById.Query(command.ClassId), ct);

            await queryBus.Send(new UserClassAccessEnsure.Query(authorId, command.ClassId), ct);

            await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.SchoolYearId), ct);

            SubjectResponse subject = await queryBus.Send(new SubjectFinderById.Query(command.SubjectId), ct);
            if (subject.ClassId != command.ClassId)
            {
                throw new AssignmentExceptions.SubjectNotInClass();
            }

            bool isClassRepresentative = await queryBus.Send(
                new UserIsClassRepresentative.Query(authorId, command.ClassId), ct);

            Assignment assignment = Assignment.Create(
                command.Id, command.ClassId, command.SubjectId, command.Title, command.Description,
                command.DueDate, authorId, isProposal: !isClassRepresentative, clock);

            await repository.AddAsync(assignment, ct);
        }
    }

    public sealed class Handler(Create create) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => create.ExecuteAsync(command, ct);
    }
}
