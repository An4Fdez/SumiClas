namespace SumiClas.Tests.ClassExpenses.Application;

using SumiClas.ClassExpenses.Application;

public static class ClassExpensePendingSummaryFinderByClassQueryMother
{
    public static ClassExpensePendingSummaryFinderByClass.Query WithClassId(Guid classId) => new(classId);
}
