using SumiClas.Shared.Domain;

namespace SumiClas.ClassExpenses.Domain;

/// <summary>Replaced, not mutated in place — same pattern as ClassRoleAssignment.</summary>
public sealed class ExpenseParticipant : ValueObject
{
    public Guid GuardianId { get; }
    public bool HasPaid { get; }

    public ExpenseParticipant(Guid guardianId, bool hasPaid)
    {
        GuardianId = guardianId;
        HasPaid = hasPaid;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return GuardianId;
        yield return HasPaid;
    }
}
