using MediatR;
using SumiClas.Assignments.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.Assignments.Application;

public static class AssignmentDoneMark
{
    /// <summary>The current user marks the homework as done. Idempotent: marking it again does nothing.</summary>
    public sealed record Command(Guid AssignmentId) : IRequest, ICommand;

    public sealed class Mark(IAssignmentRepository repository, IQueryBus queryBus, IClock clock, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            Guid current = userProvider.GetCurrentUserId();

            Assignment assignment = await repository.GetByIdAsync(command.AssignmentId, ct)
                ?? throw new AssignmentExceptions.NotFound();

            await queryBus.Send(new UserFinderById.Query(current), ct);

            await queryBus.Send(new UserClassAccessEnsure.Query(current, assignment.ClassId), ct);

            assignment.MarkAsDoneBy(current, clock);
        }
    }

    public sealed class Handler(Mark mark) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => mark.ExecuteAsync(command, ct);
    }
}
