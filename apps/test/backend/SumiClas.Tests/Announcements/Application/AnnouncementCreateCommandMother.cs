namespace SumiClas.Tests.Announcements.Application;

using SumiClas.Announcements.Application;
using SumiClas.Tests.Shared;

public static class AnnouncementCreateCommandMother
{
    public static AnnouncementCreate.Command With(
        Guid id,
        string scope,
        Guid? classId,
        string title,
        string body,
        string priority,
        Guid? relatedEventId)
        => new(id, scope, classId, title, body, priority, relatedEventId);

    public static AnnouncementCreate.Command Valid(
        Guid? id = null,
        string? scope = null,
        Guid? classId = null,
        string? title = null,
        string? body = null,
        string? priority = null,
        Guid? relatedEventId = null)
        => With(
            id ?? GuidMother.Random(),
            scope ?? "Class",
            classId ?? GuidMother.Random(),
            title ?? StringMother.Random("Notice"),
            body ?? StringMother.Random("Body"),
            priority ?? "Normal",
            relatedEventId);

    public static AnnouncementCreate.Command ClassScopeWithClassId(
        Guid? classId,
        Guid? id = null,
        string? title = null)
        => With(
            id ?? GuidMother.Random(),
            "Class",
            classId,
            title ?? StringMother.Random("Notice"),
            StringMother.Random("Body"),
            "Normal",
            null);
}
