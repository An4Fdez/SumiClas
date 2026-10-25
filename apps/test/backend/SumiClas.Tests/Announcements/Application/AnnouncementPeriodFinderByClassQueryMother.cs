namespace SumiClas.Tests.Announcements.Application;

using SumiClas.Announcements.Application;

public static class AnnouncementPeriodFinderByClassQueryMother
{
    public static AnnouncementPeriodFinderByClass.Query With(Guid classId, DateOnly from, DateOnly to)
        => new(classId, from, to);
}
