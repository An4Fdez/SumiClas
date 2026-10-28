namespace SumiClas.Tests.ClassExpenses.Application;

using SumiClas.ClassExpenses.Application;
using SumiClas.Tests.Shared;

public static class ClassExpenseUpdateCommandMother
{
    private const decimal SomeAmount = 25m;

    public static ClassExpenseUpdate.Command With(Guid id, string item, decimal totalAmount, Guid? relatedEventId)
        => new(id, item, totalAmount, relatedEventId);

    public static ClassExpenseUpdate.Command WithRelatedEvent(Guid id, string item)
        => With(id, item, SomeAmount, GuidMother.Random());
}
