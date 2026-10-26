namespace SumiClas.Tests.Announcements.Application;

using SumiClas.Announcements.Application;

public static class AnnouncementDeleteCommandMother
{
    public static AnnouncementDelete.Command WithAnnouncementId(Guid announcementId) => new(announcementId);
}
