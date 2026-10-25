namespace SumiClas.Tests.CalendarEvents.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.CalendarEvents.Application;
using SumiClas.CalendarEvents.Domain;
using SumiClas.Classes.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Users.Domain;

[TestFixture]
public class CalendarEventPeriodFinderByClassShould : CalendarEventsUnitTestCase
{
    private const int ONE_DAY = 1;
    private const int TWO_DAYS = 2;
    private const int WEEK_LENGTH_AFTER_FIRST_DAY = 6;
    private const int A_YEAR_AGO = -1;

    private static readonly DateOnly FROM = new(2026, 10, 5);
    private static readonly DateOnly TO = FROM.AddDays(WEEK_LENGTH_AFTER_FIRST_DAY);

    private readonly CalendarEventPeriodFinderByClass.Finder _useCase;

    public CalendarEventPeriodFinderByClassShould()
    {
        this._useCase = new CalendarEventPeriodFinderByClass.Finder(this.QueryBus.Object);
    }

    [Test]
    public async Task return_the_events_of_the_period_with_their_type_and_date_soonest_first()
    {
        Guid classId = GuidMother.Random();
        DateOnly later = FROM.AddDays(TWO_DAYS);
        DateOnly sooner = FROM.AddDays(ONE_DAY);
        string laterTitle = StringMother.Random("later");
        string soonerTitle = StringMother.Random("sooner");
        this.ShouldListEventsOfClass(
            classId,
            CalendarEventResponseMother.OneTime(later, laterTitle),
            CalendarEventResponseMother.OneTime(sooner, soonerTitle));

        IReadOnlyCollection<CalendarEventPeriodResponse> result = await this.InPeriodAsync(classId);

        result.Should().Equal(
            new CalendarEventPeriodResponse(soonerTitle, nameof(EventType.Meeting), sooner),
            new CalendarEventPeriodResponse(laterTitle, nameof(EventType.Meeting), later));
    }

    [Test]
    public async Task include_the_events_of_the_first_and_of_the_last_day_but_not_the_ones_just_outside()
    {
        Guid classId = GuidMother.Random();
        string first = StringMother.Random("first");
        string last = StringMother.Random("last");
        this.ShouldListEventsOfClass(
            classId,
            CalendarEventResponseMother.OneTime(FROM.AddDays(-ONE_DAY)),
            CalendarEventResponseMother.OneTime(FROM, first),
            CalendarEventResponseMother.OneTime(TO, last),
            CalendarEventResponseMother.OneTime(TO.AddDays(ONE_DAY)));

        IReadOnlyCollection<CalendarEventPeriodResponse> result = await this.InPeriodAsync(classId);

        result.Select(inPeriod => inPeriod.Title).Should().Equal(first, last);
    }

    [Test]
    public async Task include_a_birthday_with_the_date_it_has_inside_the_period()
    {
        Guid classId = GuidMother.Random();
        DateOnly day = FROM.AddDays(TWO_DAYS);
        string title = StringMother.Random("birthday");
        this.ShouldListEventsOfClass(classId, CalendarEventResponseMother.Birthday(day.Month, day.Day, title));

        IReadOnlyCollection<CalendarEventPeriodResponse> result = await this.InPeriodAsync(classId);

        result.Should().Equal(new CalendarEventPeriodResponse(title, nameof(EventType.Birthday), day));
    }

    [Test]
    public async Task not_include_a_birthday_that_falls_outside_the_period()
    {
        Guid classId = GuidMother.Random();
        DateOnly dayAfter = TO.AddDays(ONE_DAY);
        DateOnly dayBefore = FROM.AddDays(-ONE_DAY);
        this.ShouldListEventsOfClass(
            classId,
            CalendarEventResponseMother.Birthday(dayAfter.Month, dayAfter.Day),
            CalendarEventResponseMother.Birthday(dayBefore.Month, dayBefore.Day));

        IReadOnlyCollection<CalendarEventPeriodResponse> result = await this.InPeriodAsync(classId);

        result.Should().BeEmpty();
    }

    [Test]
    public async Task not_include_a_one_time_event_of_the_same_day_and_month_in_another_year()
    {
        Guid classId = GuidMother.Random();
        DateOnly sameDayLastYear = FROM.AddYears(A_YEAR_AGO);
        this.ShouldListEventsOfClass(classId, CalendarEventResponseMother.OneTime(sameDayLastYear));

        IReadOnlyCollection<CalendarEventPeriodResponse> result = await this.InPeriodAsync(classId);

        result.Should().BeEmpty();
    }

    [Test]
    public async Task order_the_events_of_the_same_day_by_title()
    {
        Guid classId = GuidMother.Random();
        DateOnly day = FROM.AddDays(TWO_DAYS);
        this.ShouldListEventsOfClass(
            classId,
            CalendarEventResponseMother.OneTime(day, "b"),
            CalendarEventResponseMother.OneTime(day, "a"),
            CalendarEventResponseMother.OneTime(day, "c"));

        IReadOnlyCollection<CalendarEventPeriodResponse> result = await this.InPeriodAsync(classId);

        result.Select(inPeriod => inPeriod.Title).Should().Equal("a", "b", "c");
    }

    [Test]
    public async Task return_an_empty_list_when_the_period_ends_before_it_starts()
    {
        Guid classId = GuidMother.Random();
        this.ShouldListEventsOfClass(classId, CalendarEventResponseMother.OneTime(FROM));

        IReadOnlyCollection<CalendarEventPeriodResponse> result = await this._useCase.ExecuteAsync(
            CalendarEventPeriodFinderByClassQueryMother.With(classId, TO, FROM),
            CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Test]
    public async Task return_an_empty_list_when_the_class_has_no_events()
    {
        Guid classId = GuidMother.Random();
        this.ShouldListEventsOfClass(classId);

        IReadOnlyCollection<CalendarEventPeriodResponse> result = await this.InPeriodAsync(classId);

        result.Should().BeEmpty();
    }

    [Test]
    public async Task ask_for_the_events_of_the_requested_class_only()
    {
        Guid classId = GuidMother.Random();
        this.ShouldListEventsOfClass(classId);

        await this.InPeriodAsync(classId);

        this.QueryBus.Verify(
            bus => bus.Send(
                It.Is<CalendarEventFinderByClass.Query>(query => query.ClassId == classId),
                It.IsAny<CancellationToken>()),
            Times.Once);
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task let_the_class_access_error_through_when_the_user_does_not_belong_to_the_class()
    {
        this.QueryBus
            .Setup(bus => bus.Send(
                It.IsAny<CalendarEventFinderByClass.Query>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UserExceptions.ClassAccessForbidden());

        Func<Task> act = () => this.InPeriodAsync(GuidMother.Random());

        await act.Should().ThrowAsync<UserExceptions.ClassAccessForbidden>();
    }

    [Test]
    public async Task let_the_class_error_through_when_the_class_is_unknown()
    {
        this.QueryBus
            .Setup(bus => bus.Send(
                It.IsAny<CalendarEventFinderByClass.Query>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ClassExceptions.NotFound());

        Func<Task> act = () => this.InPeriodAsync(GuidMother.Random());

        await act.Should().ThrowAsync<ClassExceptions.NotFound>();
    }

    private Task<IReadOnlyCollection<CalendarEventPeriodResponse>> InPeriodAsync(Guid classId)
        => this._useCase.ExecuteAsync(
            CalendarEventPeriodFinderByClassQueryMother.With(classId, FROM, TO),
            CancellationToken.None);

    private void ShouldListEventsOfClass(Guid classId, params CalendarEventResponse[] events)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<CalendarEventFinderByClass.Query>(query => query.ClassId == classId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<CalendarEventResponse>)events);
}
