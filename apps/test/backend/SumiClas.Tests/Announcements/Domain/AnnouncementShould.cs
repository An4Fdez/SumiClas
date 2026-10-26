namespace SumiClas.Tests.Announcements.Domain;

using AwesomeAssertions;
using SumiClas.Announcements.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class AnnouncementShould : UnitTestCase
{
    private static readonly TimeSpan ONE_HOUR = TimeSpan.FromHours(1);

    [Test]
    public void throw_school_required_when_a_school_announcement_has_no_school()
    {
        Func<Announcement> act = () => this.CreateWithScope(AnnouncementScope.School, GuidMother.Random(), null);

        act.Should().Throw<AnnouncementExceptions.SchoolRequiredForSchoolScope>();
    }

    [Test]
    public void throw_school_required_when_a_school_announcement_has_an_empty_school()
    {
        Func<Announcement> act = () => this.CreateWithScope(AnnouncementScope.School, GuidMother.Random(), Guid.Empty);

        act.Should().Throw<AnnouncementExceptions.SchoolRequiredForSchoolScope>();
    }

    [Test]
    public void keep_only_the_class_when_the_announcement_is_of_the_class_scope()
    {
        Guid classId = GuidMother.Random();

        Announcement announcement = this.CreateWithScope(AnnouncementScope.Class, classId, GuidMother.Random());

        announcement.ClassId.Should().Be(classId);
        announcement.SchoolId.Should().BeNull();
    }

    [Test]
    public void keep_only_the_school_when_the_announcement_is_of_the_school_scope()
    {
        Guid schoolId = GuidMother.Random();

        Announcement announcement = this.CreateWithScope(AnnouncementScope.School, GuidMother.Random(), schoolId);

        announcement.SchoolId.Should().Be(schoolId);
        announcement.ClassId.Should().BeNull();
    }

    [Test]
    public void change_the_content_trimmed_and_set_the_update_time_without_touching_the_rest()
    {
        Guid classId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        Guid eventId = GuidMother.Random();
        Announcement announcement = AnnouncementMother.With(
            Now,
            scope: AnnouncementScope.Class,
            classId: classId,
            authorId: authorId,
            relatedEventId: GuidMother.Random(),
            priority: Priority.Low);
        Guid id = announcement.Id;
        this.AdvanceClock(ONE_HOUR);
        string title = StringMother.Random("New title");
        string body = StringMother.Random("New body");

        announcement.Update($"  {title}  ", $"  {body}  ", Priority.High, eventId, this.Clock.Object);

        announcement.Title.Should().Be(title);
        announcement.Body.Should().Be(body);
        announcement.Priority.Should().Be(Priority.High);
        announcement.RelatedEventId.Should().Be(eventId);
        announcement.UpdatedAt.Should().Be(Now + ONE_HOUR);
        announcement.Id.Should().Be(id);
        announcement.Scope.Should().Be(AnnouncementScope.Class);
        announcement.ClassId.Should().Be(classId);
        announcement.AuthorId.Should().Be(authorId);
        announcement.CreatedAt.Should().Be(Now);
    }

    [Test]
    public void drop_every_read_receipt_so_it_is_unread_again_for_everybody()
    {
        Announcement announcement = AnnouncementMother.ReadBy(GuidMother.Random(), GuidMother.Random(), Now);
        announcement.MarkAsReadBy(GuidMother.Random(), this.Clock.Object);

        announcement.Update(
            StringMother.Random("Notice"),
            StringMother.Random("Body"),
            Priority.Normal,
            null,
            this.Clock.Object);

        announcement.ReadReceipts.Should().BeEmpty();
    }

    [TestCase("")]
    [TestCase("   ")]
    public void throw_title_required_without_changing_anything_when_the_new_title_is_blank(string title)
    {
        Announcement announcement = AnnouncementMother.ReadBy(GuidMother.Random(), GuidMother.Random(), Now);
        string originalTitle = announcement.Title;

        Action act = () => announcement.Update(
            title,
            StringMother.Random("Body"),
            Priority.High,
            null,
            this.Clock.Object);

        act.Should().Throw<AnnouncementExceptions.TitleRequired>();
        announcement.Title.Should().Be(originalTitle);
        announcement.Priority.Should().Be(Priority.Normal);
        announcement.UpdatedAt.Should().BeNull();
        announcement.ReadReceipts.Should().ContainSingle();
    }

    [TestCase("")]
    [TestCase("   ")]
    public void throw_body_required_without_changing_anything_when_the_new_body_is_blank(string body)
    {
        Announcement announcement = AnnouncementMother.ReadBy(GuidMother.Random(), GuidMother.Random(), Now);
        string originalTitle = announcement.Title;

        Action act = () => announcement.Update(
            StringMother.Random("New title"),
            body,
            Priority.High,
            null,
            this.Clock.Object);

        act.Should().Throw<AnnouncementExceptions.BodyRequired>();
        announcement.Title.Should().Be(originalTitle);
        announcement.UpdatedAt.Should().BeNull();
        announcement.ReadReceipts.Should().ContainSingle();
    }

    private Announcement CreateWithScope(AnnouncementScope scope, Guid? classId, Guid? schoolId)
        => Announcement.Create(
            GuidMother.Random(),
            scope,
            classId,
            schoolId,
            StringMother.Random("Notice"),
            StringMother.Random("Body"),
            Priority.Normal,
            GuidMother.Random(),
            null,
            this.Clock.Object);
}
