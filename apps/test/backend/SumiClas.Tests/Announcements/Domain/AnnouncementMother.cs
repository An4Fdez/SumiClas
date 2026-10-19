namespace SumiClas.Tests.Announcements.Domain;

using Moq;
using SumiClas.Announcements.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;

public static class AnnouncementMother
{
    public static Announcement With(
        DateTimeOffset createdAt,
        Guid? id = null,
        AnnouncementScope? scope = null,
        Guid? classId = null,
        string? title = null,
        Guid? authorId = null,
        Guid? relatedEventId = null)
        => new(
            id ?? GuidMother.Random(),
            scope ?? AnnouncementScope.School,
            classId,
            title ?? StringMother.Random("Notice"),
            StringMother.Random("Body"),
            Priority.Normal,
            authorId ?? GuidMother.Random(),
            relatedEventId,
            createdAt);

    public static Announcement WithId(Guid id) => With(UnitTestCase.Now, id: id);

    public static Announcement ReadBy(Guid readerId, DateTimeOffset createdAt)
    {
        Mock<IClock> clock = new();
        clock.Setup(source => source.UtcNow).Returns(UnitTestCase.Now);
        Announcement announcement = With(createdAt);
        announcement.MarkAsReadBy(readerId, clock.Object);
        return announcement;
    }

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
