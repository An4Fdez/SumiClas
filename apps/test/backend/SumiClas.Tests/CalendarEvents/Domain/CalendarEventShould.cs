namespace SumiClas.Tests.CalendarEvents.Domain;

using AwesomeAssertions;
using SumiClas.CalendarEvents.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class CalendarEventShould : UnitTestCase
{
    private static readonly DateOnly SomeDay = DateOnly.FromDateTime(Now.UtcDateTime).AddDays(30);

    [Test]
    public void throw_school_required_when_a_school_event_has_no_school()
    {
        Func<CalendarEvent> act = () => this.CreateWithScope(EventScope.School, GuidMother.Random(), null);

        act.Should().Throw<CalendarEventExceptions.SchoolRequiredForSchoolScope>();
    }

    [Test]
    public void throw_school_required_when_a_school_event_has_an_empty_school()
    {
        Func<CalendarEvent> act = () => this.CreateWithScope(EventScope.School, GuidMother.Random(), Guid.Empty);

        act.Should().Throw<CalendarEventExceptions.SchoolRequiredForSchoolScope>();
    }

    [Test]
    public void keep_only_the_class_when_the_event_is_of_the_class_scope()
    {
        Guid classId = GuidMother.Random();

        CalendarEvent calendarEvent = this.CreateWithScope(EventScope.Class, classId, GuidMother.Random());

        calendarEvent.ClassId.Should().Be(classId);
        calendarEvent.SchoolId.Should().BeNull();
    }

    [Test]
    public void keep_only_the_school_when_the_event_is_of_the_school_scope()
    {
        Guid schoolId = GuidMother.Random();

        CalendarEvent calendarEvent = this.CreateWithScope(EventScope.School, GuidMother.Random(), schoolId);

        calendarEvent.SchoolId.Should().Be(schoolId);
        calendarEvent.ClassId.Should().BeNull();
    }

    private CalendarEvent CreateWithScope(EventScope scope, Guid? classId, Guid? schoolId)
        => CalendarEvent.Create(
            GuidMother.Random(),
            scope,
            classId,
            schoolId,
            EventType.Trip,
            StringMother.Random("Event"),
            SomeDay,
            null,
            null,
            null,
            GuidMother.Random(),
            this.Clock.Object);
}
