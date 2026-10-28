namespace SumiClas.Tests.ClassExpenses.Domain;

using SumiClas.ClassExpenses.Domain;
using SumiClas.Tests.Shared;

public static class ClassExpenseMother
{
    public static ClassExpense With(
        Guid? id = null,
        Guid? classId = null,
        Guid? relatedEventId = null,
        Guid? authorId = null,
        DateTimeOffset? createdAt = null,
        IReadOnlyCollection<ExpenseParticipant>? participants = null,
        string? item = null)
        => new(
            id ?? GuidMother.Random(),
            classId ?? GuidMother.Random(),
            item ?? StringMother.Random("Expense"),
            Random.Shared.Next(1, 1000),
            relatedEventId,
            authorId ?? GuidMother.Random(),
            createdAt ?? UnitTestCase.Now,
            participants ?? [ExpenseParticipantMother.Unpaid(GuidMother.Random())],
            null);
}
