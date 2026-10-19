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
        Guid? schoolId = null,
        string? title = null,
        Guid? authorId = null,
        Guid? relatedEventId = null)
        => new(
            id ?? GuidMother.Random(),
            scope ?? AnnouncementScope.School,
            classId,
            schoolId ?? ((scope ?? AnnouncementScope.School) == AnnouncementScope.School ? GuidMother.Random() : null),
            title ?? StringMother.Random("Notice"),
            StringMother.Random("Body"),
            Priority.Normal,
            authorId ?? GuidMother.Random(),
            relatedEventId,
            createdAt);

    public static Announcement WithId(Guid id) => With(UnitTestCase.Now, id: id);

    public static Announcement ReadBy(Guid schoolId, Guid readerId, DateTimeOffset createdAt)
    {
        Mock<IClock> clock = new();
        clock.Setup(source => source.UtcNow).Returns(UnitTestCase.Now);
        Announcement announcement = With(createdAt, schoolId: schoolId);
        announcement.MarkAsReadBy(readerId, clock.Object);
        return announcement;
    }

    public static Announcement SchoolWide(Guid schoolId, string title, DateTimeOffset createdAt, Guid? authorId = null)
        => With(schoolId: schoolId, title: title, createdAt: createdAt, authorId: authorId);

    public static Announcement ForClass(Guid classId, string title, DateTimeOffset createdAt, Guid? authorId = null)
        => With(
            scope: AnnouncementScope.Class,
            classId: classId,
            title: title,
            createdAt: createdAt,
            authorId: authorId);
}
