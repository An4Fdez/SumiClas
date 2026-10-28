namespace SumiClas.Tests.CalendarEvents.Application;

using AwesomeAssertions;
using SumiClas.CalendarEvents.Application;
using SumiClas.CalendarEvents.Domain;
using SumiClas.Tests.CalendarEvents.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class CalendarEventUpdateShould : CalendarEventsUnitTestCase
{
    private readonly CalendarEventUpdate.Update _useCase;

    public CalendarEventUpdateShould()
    {
        this._useCase = new CalendarEventUpdate.Update(
            this.Repository.Object,
            this.QueryBus.Object,
            this.Clock.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task throw_title_required_without_asking_the_bus_when_the_new_title_is_blank()
    {
        Guid authorId = GuidMother.Random();
        DateOnly date = DateOnly.FromDateTime(Now.UtcDateTime);
        CalendarEvent calendarEvent = CalendarEventMother.ForClass(
            GuidMother.Random(),
            StringMother.Random("Event"),
            date,
            authorId);
        string title = calendarEvent.Title;
        this.ShouldFetchCalendarEvents(calendarEvent);
        this.ShouldResolveCurrentUser(authorId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            CalendarEventUpdateCommandMother.Blank(calendarEvent.Id, date));

        await act.Should().ThrowAsync<CalendarEventExceptions.TitleRequired>();
        calendarEvent.Title.Should().Be(title);
        this.QueryBus.VerifyNoOtherCalls();
    }
}
