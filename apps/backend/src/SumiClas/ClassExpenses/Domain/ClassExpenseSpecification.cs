using SumiClas.Shared.Domain;

namespace SumiClas.ClassExpenses.Domain;

public sealed class ClassExpenseSpecification : Specification<ClassExpense>
{
    /// <param name="participantId">Matches expenses that include this guardian as a participant.</param>
    /// <param name="participantHasPaid">Matches expenses with a participant that has (or has not) paid (the same participant when <paramref name="participantId"/> is also given).</param>
    public ClassExpenseSpecification(
        Guid? id = null,
        Guid? classId = null,
        string? item = null,
        decimal? totalAmount = null,
        Guid? relatedEventId = null,
        Guid? authorId = null,
        DateTimeOffset? createdAt = null,
        Guid? participantId = null,
        bool? participantHasPaid = null,
        KeysetPage<DateTimeOffset>? page = null)
    {
        if (id is { } expenseId) AddCriteria(expense => expense.Id == expenseId);
        if (classId is { } schoolClass) AddCriteria(expense => expense.ClassId == schoolClass);
        if (item is not null) AddCriteria(expense => expense.Item == item);
        if (totalAmount is { } amount) AddCriteria(expense => expense.TotalAmount == amount);
        if (relatedEventId is { } relatedEvent) AddCriteria(expense => expense.RelatedEventId == relatedEvent);
        if (authorId is { } author) AddCriteria(expense => expense.AuthorId == author);
        if (createdAt is { } created) AddCriteria(expense => expense.CreatedAt == created);
        if (participantId is not null || participantHasPaid is not null)
        {
            AddCriteria(expense => expense.Participants.Any(participant =>
                (participantId == null || participant.GuardianId == participantId)
                && (participantHasPaid == null || participant.HasPaid == participantHasPaid)));
        }

        if (page is not null) AddKeysetPage(expense => expense.CreatedAt, expense => expense.Id, page, descending: true);
    }
}
