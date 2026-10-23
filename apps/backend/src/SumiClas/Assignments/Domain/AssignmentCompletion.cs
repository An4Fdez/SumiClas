using SumiClas.Shared.Domain;

namespace SumiClas.Assignments.Domain;

/// <summary>
/// Child entity of the Assignment aggregate — not an aggregate root itself.
/// Only Assignment (via MarkAsDoneBy) can create one; it protects the invariant
/// "at most one completion per user" internally, since both live in the same aggregate.
/// </summary>
public sealed class AssignmentCompletion : Entity
{
    public Guid UserId { get; private set; }
    public DateTimeOffset CompletedAt { get; private set; }

    private AssignmentCompletion(Guid id, Guid userId, DateTimeOffset completedAt) : base(id)
    {
        UserId = userId;
        CompletedAt = completedAt;
    }

    internal static AssignmentCompletion Create(Guid userId, IClock clock)
    {
        if (userId == Guid.Empty)
        {
            throw new AssignmentExceptions.CompletionUserRequired();
        }

        return new AssignmentCompletion(Guid.NewGuid(), userId, clock.UtcNow);
    }
}
