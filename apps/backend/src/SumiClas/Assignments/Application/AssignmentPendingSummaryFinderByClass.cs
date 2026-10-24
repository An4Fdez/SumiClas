using MediatR;
using SumiClas.Assignments.Domain;
using SumiClas.Classes.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.Assignments.Application;

public static class AssignmentPendingSummaryFinderByClass
{
    /// <summary>
    /// The homework of the class that the current user has not marked as done and that is due today or later, official
    /// or proposed, and the earliest due date among them. "Today" is the UTC date of the clock.
    /// </summary>
    public sealed record Query(Guid ClassId) : IQuery<AssignmentPendingSummaryResponse>;

    public sealed class Finder(IAssignmentRepository repository, IQueryBus queryBus, IUserProvider userProvider, IClock clock)
        : IUseCase
    {
        public async Task<AssignmentPendingSummaryResponse> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            Guid current = userProvider.GetCurrentUserId();

            await queryBus.Send(new ClassFinderById.Query(query.ClassId), ct);

            await queryBus.Send(new UserClassAccessEnsure.Query(current, query.ClassId), ct);

            DateOnly today = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);

            IReadOnlyCollection<Assignment> pending = await repository.SearchAsync(
                new AssignmentSpecification(classId: query.ClassId, dueFrom: today, notDoneByUserId: current), ct);

            return new AssignmentPendingSummaryResponse(
                pending.Count,
                pending.Count == 0 ? null : pending.Min(assignment => assignment.DueDate));
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, AssignmentPendingSummaryResponse>
    {
        public Task<AssignmentPendingSummaryResponse> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
