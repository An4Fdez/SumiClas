namespace SumiClas.Tests.Announcements.Application;

using SumiClas.Announcements.Application;
using SumiClas.Tests.Shared;

public static class AnnouncementUpdateCommandMother
{
    public static AnnouncementUpdate.Command With(
        Guid id,
        string title,
        string body,
        string priority,
        Guid? relatedEventId)
        => new(id, title, body, priority, relatedEventId);

    public static AnnouncementUpdate.Command Valid(
        Guid id,
        string? title = null,
        string? body = null,
        string? priority = null,
        Guid? relatedEventId = null)
        => With(
            id,
            title ?? StringMother.Random("Notice"),
            body ?? StringMother.Random("Body"),
            priority ?? "Normal",
            relatedEventId);
}
