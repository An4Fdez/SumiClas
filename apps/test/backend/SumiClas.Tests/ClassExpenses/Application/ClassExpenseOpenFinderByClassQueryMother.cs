namespace SumiClas.Tests.ClassExpenses.Application;

using SumiClas.ClassExpenses.Application;

public static class ClassExpenseOpenFinderByClassQueryMother
{
    public static ClassExpenseOpenFinderByClass.Query WithClassId(Guid classId) => new(classId);
}
