namespace SumiClas.Tests.Announcements.Application;

using SumiClas.Announcements.Application;

public static class AnnouncementReadMarkCommandMother
{
    public static AnnouncementReadMark.Command WithAnnouncementId(Guid announcementId) => new(announcementId);
}
