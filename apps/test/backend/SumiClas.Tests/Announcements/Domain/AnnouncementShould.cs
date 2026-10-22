namespace SumiClas.Tests.Announcements.Domain;

using AwesomeAssertions;
using SumiClas.Announcements.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class AnnouncementShould : UnitTestCase
{
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
