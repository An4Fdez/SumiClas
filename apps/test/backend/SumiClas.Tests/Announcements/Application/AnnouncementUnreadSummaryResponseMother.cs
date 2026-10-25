namespace SumiClas.Tests.Announcements.Application;

using SumiClas.Announcements.Application;

public static class AnnouncementUnreadSummaryResponseMother
{
    public static AnnouncementUnreadSummaryResponse None() => new(Unread: 0, UnreadUrgent: 0);
}
