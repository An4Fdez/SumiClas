namespace SumiClas.Tests.CalendarEvents.Application;

using AwesomeAssertions;
using SumiClas.CalendarEvents.Application;
using SumiClas.Tests.CalendarEvents.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class CalendarEventFinderByIdsShould : CalendarEventsUnitTestCase
{
    private readonly CalendarEventFinderByIds.Finder _useCase;

    public CalendarEventFinderByIdsShould()
    {
        this._useCase = new CalendarEventFinderByIds.Finder(this.Repository.Object);
    }

    [Test]
    public async Task return_the_id_and_title_of_the_requested_events_only()
    {
        Guid firstId = GuidMother.Random();
        Guid secondId = GuidMother.Random();
        string firstTitle = StringMother.Random("Event");
        string secondTitle = StringMother.Random("Event");
        this.ShouldSearchCalendarEvents(
            CalendarEventMother.With(firstId, firstTitle),
            CalendarEventMother.WithId(GuidMother.Random()),
            CalendarEventMother.With(secondId, secondTitle));

        IReadOnlyCollection<CalendarEventSummaryResponse> result = await this._useCase.ExecuteAsync(
            CalendarEventFinderByIdsQueryMother.WithIds(firstId, secondId));

        result.Should().BeEquivalentTo(new[]
        {
            CalendarEventSummaryResponseMother.With(firstId, firstTitle),
            CalendarEventSummaryResponseMother.With(secondId, secondTitle),
        });
    }

    [Test]
    public async Task skip_the_ids_that_do_not_exist()
    {
        Guid existingId = GuidMother.Random();
        string title = StringMother.Random("Event");
        this.ShouldSearchCalendarEvents(CalendarEventMother.With(existingId, title));

        IReadOnlyCollection<CalendarEventSummaryResponse> result = await this._useCase.ExecuteAsync(
            CalendarEventFinderByIdsQueryMother.WithIds(GuidMother.Random(), existingId, GuidMother.Random()));

        result.Should().ContainSingle().Which.Should()
            .Be(CalendarEventSummaryResponseMother.With(existingId, title));
    }

    [Test]
    public async Task return_nothing_when_no_ids_are_requested()
    {
        this.ShouldSearchCalendarEvents(CalendarEventMother.WithId(GuidMother.Random()));

        IReadOnlyCollection<CalendarEventSummaryResponse> result = await this._useCase.ExecuteAsync(
            CalendarEventFinderByIdsQueryMother.WithIds());

        result.Should().BeEmpty();
    }
}
