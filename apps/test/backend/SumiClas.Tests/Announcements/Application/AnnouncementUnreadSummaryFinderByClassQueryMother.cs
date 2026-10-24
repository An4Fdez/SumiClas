namespace SumiClas.Tests.Announcements.Application;

using SumiClas.Announcements.Application;

public static class AnnouncementUnreadSummaryFinderByClassQueryMother
{
    public static AnnouncementUnreadSummaryFinderByClass.Query WithClassId(Guid classId) => new(classId);

    public static AnnouncementUnreadSummaryFinderByClass.Query WithDays(Guid classId, int days) => new(classId, days);
}
