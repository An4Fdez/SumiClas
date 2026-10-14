namespace SumiClas.Tests.ClassExpenses.Application;

using SumiClas.ClassExpenses.Application;
using SumiClas.Tests.Shared;

public static class ClassExpenseCreateCommandMother
{
    public static ClassExpenseCreate.Command With(
        Guid id,
        Guid classId,
        string item,
        decimal totalAmount,
        IReadOnlyCollection<Guid> participantIds,
        Guid? relatedEventId)
        => new(id, classId, item, totalAmount, participantIds, relatedEventId);

    public static ClassExpenseCreate.Command Valid(
        Guid? id = null,
        Guid? classId = null,
        string? item = null,
        decimal? totalAmount = null,
        IReadOnlyCollection<Guid>? participantIds = null,
        Guid? relatedEventId = null)
        => With(
            id ?? GuidMother.Random(),
            classId ?? GuidMother.Random(),
            item ?? StringMother.Random("Expense"),
            totalAmount ?? Random.Shared.Next(1, 1000),
            participantIds ?? [GuidMother.Random()],
            relatedEventId);
}
