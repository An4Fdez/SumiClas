namespace SumiClas.Tests.ClassExpenses.Application;

using SumiClas.ClassExpenses.Application;

public static class ClassExpenseFinderByClassQueryMother
{
    public static ClassExpenseFinderByClass.Query With(Guid classId, int? limit, string? cursor)
        => new(classId, limit, cursor);

    public static ClassExpenseFinderByClass.Query WithClassId(Guid classId) => With(classId, null, null);

    public static ClassExpenseFinderByClass.Query WithLimit(Guid classId, int limit) => With(classId, limit, null);

    public static ClassExpenseFinderByClass.Query WithCursor(Guid classId, int limit, string cursor)
        => With(classId, limit, cursor);
}
