namespace SumiClas.Tests.LostItems.Application;

using SumiClas.LostItems.Application;

public static class LostItemFinderByClassQueryMother
{
    public static LostItemFinderByClass.Query With(Guid classId, int? limit, string? cursor)
        => new(classId, limit, cursor);

    public static LostItemFinderByClass.Query WithClassId(Guid classId) => With(classId, null, null);

    public static LostItemFinderByClass.Query WithLimit(Guid classId, int limit) => With(classId, limit, null);

    public static LostItemFinderByClass.Query WithCursor(Guid classId, int limit, string cursor)
        => With(classId, limit, cursor);
}
