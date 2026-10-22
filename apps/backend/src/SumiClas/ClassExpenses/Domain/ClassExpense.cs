using SumiClas.Shared.Domain;

namespace SumiClas.ClassExpenses.Domain;

public sealed class ClassExpense : AggregateRoot
{
    private readonly List<ExpenseParticipant> _participants = new();
    public IReadOnlyCollection<ExpenseParticipant> Participants => _participants.AsReadOnly();

    /// <summary>Expenses always belong to a specific class, never school-wide.</summary>
    public Guid ClassId { get; private set; }

    public string Item { get; private set; }
    public decimal TotalAmount { get; private set; }

    /// <summary>Optional cross-module reference to a CalendarEvents module CalendarEvent.</summary>
    public Guid? RelatedEventId { get; private set; }

    public Guid AuthorId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public ClassExpense(
        Guid id,
        Guid classId,
        string item,
        decimal totalAmount,
        Guid? relatedEventId,
        Guid authorId,
        DateTimeOffset createdAt,
        IEnumerable<ExpenseParticipant> participants) : base(id)
    {
        ClassId = classId;
        Item = item;
        TotalAmount = totalAmount;
        RelatedEventId = relatedEventId;
        AuthorId = authorId;
        CreatedAt = createdAt;
        _participants.AddRange(participants);
    }

    private ClassExpense()
    {
        // For reflection purposes (EF Core materialization)
    }

    public static ClassExpense Create(
        Guid id,
        Guid classId,
        string item,
        decimal totalAmount,
        IReadOnlyCollection<Guid> participantIds,
        Guid? relatedEventId,
        Guid authorId,
        IClock clock)
    {
        var distinctParticipantIds = participantIds.Where(participantId => participantId != Guid.Empty).Distinct().ToList();

        Validate(id, classId, item, totalAmount, distinctParticipantIds, authorId);

        return new ClassExpense(
            id,
            classId,
            item.Trim(),
            totalAmount,
            relatedEventId,
            authorId,
            clock.UtcNow,
            distinctParticipantIds.Select(guardianId => new ExpenseParticipant(guardianId, false)));
    }

    private static void Validate(Guid id, Guid classId, string item, decimal totalAmount, IReadOnlyCollection<Guid> distinctParticipantIds, Guid authorId)
    {
        if (id == Guid.Empty)
        {
            throw new ClassExpenseExceptions.IdRequired();
        }
        if (classId == Guid.Empty)
        {
            throw new ClassExpenseExceptions.ClassRequired();
        }
        if (string.IsNullOrWhiteSpace(item))
        {
            throw new ClassExpenseExceptions.ItemRequired();
        }
        if (totalAmount <= 0)
        {
            throw new ClassExpenseExceptions.TotalAmountMustBePositive();
        }
        if (authorId == Guid.Empty)
        {
            throw new ClassExpenseExceptions.AuthorRequired();
        }
        if (distinctParticipantIds.Count == 0)
        {
            throw new ClassExpenseExceptions.AtLeastOneParticipantRequired();
        }
    }

    public void MarkAsPaid(Guid guardianId) => SetPaidStatus(guardianId, true);

    public void MarkAsUnpaid(Guid guardianId) => SetPaidStatus(guardianId, false);

    private void SetPaidStatus(Guid guardianId, bool hasPaid)
    {
        var index = _participants.FindIndex(p => p.GuardianId == guardianId);
        if (index < 0)
        {
            throw new ClassExpenseExceptions.NotAParticipant();
        }

        _participants[index] = new ExpenseParticipant(guardianId, hasPaid);
    }
}
