namespace SumiClas.Tests.CalendarEvents.Application;

using AwesomeAssertions;
using SumiClas.CalendarEvents.Application;
using SumiClas.CalendarEvents.Domain;
using SumiClas.CalendarEvents.Domain.Services;
using SumiClas.Shared.Domain;
using SumiClas.Tests.CalendarEvents.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class CalendarEventFinderByIdShould : CalendarEventsUnitTestCase
{
    private readonly CalendarEventFinderById.Finder _useCase;

    public CalendarEventFinderByIdShould()
    {
        this._useCase = new CalendarEventFinderById.Finder(new CalendarEventByIdFinder(this.Repository.Object));
    }

    [Test]
    public async Task return_the_id_and_title_of_the_requested_event()
    {
        Guid calendarEventId = GuidMother.Random();
        string title = StringMother.Random("Event");
        this.ShouldFetchCalendarEvents(
            CalendarEventMother.WithId(GuidMother.Random()),
            CalendarEventMother.With(calendarEventId, title),
            CalendarEventMother.WithId(GuidMother.Random()));

        CalendarEventSummaryResponse result = await this._useCase.ExecuteAsync(
            CalendarEventFinderByIdQueryMother.WithId(calendarEventId));

        result.Should().Be(CalendarEventSummaryResponseMother.With(calendarEventId, title));
    }

    [Test]
    public async Task throw_not_found_when_the_event_does_not_exist()
    {
        this.ShouldFetchCalendarEvents(CalendarEventMother.WithId(GuidMother.Random()));

        Func<Task> act = () => this._useCase.ExecuteAsync(
            CalendarEventFinderByIdQueryMother.WithId(GuidMother.Random()));

        await act.Should().ThrowAsync<CalendarEventExceptions.NotFound>();
    }
}
