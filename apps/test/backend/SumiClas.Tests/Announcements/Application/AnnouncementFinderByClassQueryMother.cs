namespace SumiClas.Tests.Announcements.Application;

using SumiClas.Announcements.Application;

public static class AnnouncementFinderByClassQueryMother
{
    public static AnnouncementFinderByClass.Query With(Guid classId, int? limit, string? cursor)
        => new(classId, limit, cursor);

    public static AnnouncementFinderByClass.Query WithClassId(Guid classId) => With(classId, null, null);

    public static AnnouncementFinderByClass.Query WithLimit(Guid classId, int limit) => With(classId, limit, null);

    public static AnnouncementFinderByClass.Query WithCursor(Guid classId, int limit, string cursor)
        => With(classId, limit, cursor);
}
