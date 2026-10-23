using MediatR;
using SumiClas.Assignments.Domain;
using SumiClas.Shared.Application;
using SumiClas.Users.Application;

namespace SumiClas.Assignments.Application;

public static class AssignmentDoneUnmark
{
    /// <summary>The current user takes back their done mark. Idempotent: unmarking it again does nothing.</summary>
    public sealed record Command(Guid AssignmentId) : IRequest, ICommand;

    public sealed class Unmark(IAssignmentRepository repository, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            Guid current = userProvider.GetCurrentUserId();

            Assignment assignment = await repository.GetByIdAsync(command.AssignmentId, ct)
                ?? throw new AssignmentExceptions.NotFound();

            await queryBus.Send(new UserFinderById.Query(current), ct);

            await queryBus.Send(new UserClassAccessEnsure.Query(current, assignment.ClassId), ct);

            assignment.UnmarkAsDoneBy(current);
        }
    }

    public sealed class Handler(Unmark unmark) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => unmark.ExecuteAsync(command, ct);
    }
}
