namespace SumiClas.Tests.Announcements.Domain;

using SumiClas.Announcements.Domain;
using SumiClas.Tests.Shared;

public static class AnnouncementMother
{
    public static Announcement With(
        DateTimeOffset createdAt,
        Guid? id = null,
        AnnouncementScope? scope = null,
        Guid? classId = null,
        string? title = null,
        Guid? authorId = null)
        => new(
            id ?? GuidMother.Random(),
            scope ?? AnnouncementScope.School,
            classId,
            title ?? StringMother.Random("Notice"),
            StringMother.Random("Body"),
            Priority.Normal,
            authorId ?? GuidMother.Random(),
            null,
            createdAt);

    public static Announcement SchoolWide(string title, DateTimeOffset createdAt, Guid? authorId = null)
        => With(title: title, createdAt: createdAt, authorId: authorId);

    public static Announcement ForClass(Guid classId, string title, DateTimeOffset createdAt, Guid? authorId = null)
        => With(
            scope: AnnouncementScope.Class,
            classId: classId,
            title: title,
            createdAt: createdAt,
            authorId: authorId);
}
