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

    public static Result<ClassExpense> Create(
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

        var validation = Validate(id, classId, item, totalAmount, distinctParticipantIds, authorId);
        if (validation.IsFailure)
        {
            return Result<ClassExpense>.Failure(validation.Error!);
        }

        var expense = new ClassExpense(
            id,
            classId,
            item.Trim(),
            totalAmount,
            relatedEventId,
            authorId,
            clock.UtcNow,
            distinctParticipantIds.Select(guardianId => new ExpenseParticipant(guardianId, false)));

        return Result<ClassExpense>.Success(expense);
    }

    private static Result Validate(Guid id, Guid classId, string item, decimal totalAmount, IReadOnlyCollection<Guid> distinctParticipantIds, Guid authorId)
    {
        if (id == Guid.Empty)
        {
            return Result.Failure(ClassExpenseErrors.IdRequired);
        }
        if (classId == Guid.Empty)
        {
            return Result.Failure(ClassExpenseErrors.ClassRequired);
        }
        if (string.IsNullOrWhiteSpace(item))
        {
            return Result.Failure(ClassExpenseErrors.ItemRequired);
        }
        if (totalAmount <= 0)
        {
            return Result.Failure(ClassExpenseErrors.TotalAmountMustBePositive);
        }
        if (authorId == Guid.Empty)
        {
            return Result.Failure(ClassExpenseErrors.AuthorRequired);
        }
        if (distinctParticipantIds.Count == 0)
        {
            return Result.Failure(ClassExpenseErrors.AtLeastOneParticipantRequired);
        }
        return Result.Success();
    }

    public Result MarkAsPaid(Guid guardianId) => SetPaidStatus(guardianId, true);

    public Result MarkAsUnpaid(Guid guardianId) => SetPaidStatus(guardianId, false);

    private Result SetPaidStatus(Guid guardianId, bool hasPaid)
    {
        var index = _participants.FindIndex(p => p.GuardianId == guardianId);
        if (index < 0)
        {
            return Result.Failure(ClassExpenseErrors.NotAParticipant);
        }

        _participants[index] = new ExpenseParticipant(guardianId, hasPaid);
        return Result.Success();
    }
}
