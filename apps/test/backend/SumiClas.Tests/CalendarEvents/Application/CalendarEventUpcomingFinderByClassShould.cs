namespace SumiClas.Tests.CalendarEvents.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.CalendarEvents.Application;
using SumiClas.CalendarEvents.Domain;
using SumiClas.Classes.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Users.Domain;

[TestFixture]
public class CalendarEventUpcomingFinderByClassShould : CalendarEventsUnitTestCase
{
    private const int REQUESTED_DAYS = 7;
    private const int ONE_DAY = 1;
    private const int TWO_DAYS = 2;
    private const int THREE_DAYS = 3;

    private static readonly DateOnly TODAY = DateOnly.FromDateTime(Now.UtcDateTime);
    private static readonly DateOnly YESTERDAY = TODAY.AddDays(-ONE_DAY);
    private static readonly DateOnly LAST_DAY_OF_REQUESTED = TODAY.AddDays(REQUESTED_DAYS);
    private static readonly DateOnly LAST_DAY_OF_DEFAULT = TODAY.AddDays(SummaryPeriod.DefaultDays);
    private static readonly DateOnly LAST_DAY_OF_MAXIMUM = TODAY.AddDays(SummaryPeriod.MaxDays);

    private readonly CalendarEventUpcomingFinderByClass.Finder _useCase;

    public CalendarEventUpcomingFinderByClassShould()
    {
        this._useCase = new CalendarEventUpcomingFinderByClass.Finder(this.QueryBus.Object, this.Clock.Object);
    }

    [Test]
    public async Task return_the_events_ahead_with_their_type_date_and_days_until_soonest_first()
    {
        Guid classId = GuidMother.Random();
        DateOnly later = TODAY.AddDays(THREE_DAYS);
        DateOnly sooner = TODAY.AddDays(TWO_DAYS);
        string laterTitle = StringMother.Random("later");
        string soonerTitle = StringMother.Random("sooner");
        this.ShouldListEventsOfClass(
            classId,
            CalendarEventResponseMother.OneTime(later, laterTitle),
            CalendarEventResponseMother.OneTime(sooner, soonerTitle));

        IReadOnlyCollection<CalendarEventUpcomingResponse> result = await this.UpcomingAsync(classId);

        result.Should().Equal(
            CalendarEventUpcomingResponseMother.With(nameof(EventType.Meeting), sooner, TWO_DAYS, soonerTitle),
            CalendarEventUpcomingResponseMother.With(nameof(EventType.Meeting), later, THREE_DAYS, laterTitle));
    }

    [Test]
    public async Task include_the_events_of_today_with_zero_days_until()
    {
        Guid classId = GuidMother.Random();
        string title = StringMother.Random("today");
        this.ShouldListEventsOfClass(classId, CalendarEventResponseMother.OneTime(TODAY, title));

        IReadOnlyCollection<CalendarEventUpcomingResponse> result = await this.UpcomingAsync(classId);

        result.Should().Equal(
            CalendarEventUpcomingResponseMother.With(nameof(EventType.Meeting), TODAY, daysUntil: 0, title));
    }

    [Test]
    public async Task not_include_the_one_time_events_already_over()
    {
        Guid classId = GuidMother.Random();
        this.ShouldListEventsOfClass(classId, CalendarEventResponseMother.OneTime(YESTERDAY));

        IReadOnlyCollection<CalendarEventUpcomingResponse> result = await this.UpcomingAsync(classId);

        result.Should().BeEmpty();
    }

    [Test]
    public async Task include_a_birthday_with_its_next_date_when_it_falls_inside_the_window()
    {
        Guid classId = GuidMother.Random();
        string title = StringMother.Random("birthday");
        this.ShouldListEventsOfClass(
            classId,
            CalendarEventResponseMother.Birthday(LAST_DAY_OF_DEFAULT.Month, LAST_DAY_OF_DEFAULT.Day, title));

        IReadOnlyCollection<CalendarEventUpcomingResponse> result = await this.UpcomingAsync(classId);

        result.Should().Equal(CalendarEventUpcomingResponseMother.With(
            nameof(EventType.Birthday),
            LAST_DAY_OF_DEFAULT,
            SummaryPeriod.DefaultDays,
            title));
    }

    [Test]
    public async Task not_include_a_birthday_that_just_passed_because_its_next_one_is_a_year_away()
    {
        Guid classId = GuidMother.Random();
        this.ShouldListEventsOfClass(
            classId,
            CalendarEventResponseMother.Birthday(YESTERDAY.Month, YESTERDAY.Day));

        IReadOnlyCollection<CalendarEventUpcomingResponse> result = await this._useCase.ExecuteAsync(
            CalendarEventUpcomingFinderByClassQueryMother.WithDays(classId, SummaryPeriod.MaxDays),
            CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Test]
    public async Task include_the_events_up_to_exactly_the_last_day_of_the_default_window_but_not_beyond()
    {
        Guid classId = GuidMother.Random();
        string inside = StringMother.Random("inside");
        this.ShouldListEventsOfClass(
            classId,
            CalendarEventResponseMother.OneTime(LAST_DAY_OF_DEFAULT, inside),
            CalendarEventResponseMother.OneTime(LAST_DAY_OF_DEFAULT.AddDays(ONE_DAY)));

        IReadOnlyCollection<CalendarEventUpcomingResponse> result = await this.UpcomingAsync(classId);

        result.Select(upcoming => upcoming.Title).Should().Equal(inside);
    }

    [Test]
    public async Task look_ahead_only_as_many_days_as_requested()
    {
        Guid classId = GuidMother.Random();
        string inside = StringMother.Random("inside");
        this.ShouldListEventsOfClass(
            classId,
            CalendarEventResponseMother.OneTime(LAST_DAY_OF_REQUESTED, inside),
            CalendarEventResponseMother.OneTime(LAST_DAY_OF_REQUESTED.AddDays(ONE_DAY)));

        IReadOnlyCollection<CalendarEventUpcomingResponse> result = await this._useCase.ExecuteAsync(
            CalendarEventUpcomingFinderByClassQueryMother.WithDays(classId, REQUESTED_DAYS),
            CancellationToken.None);

        result.Select(upcoming => upcoming.Title).Should().Equal(inside);
    }

    [Test]
    public async Task look_ahead_up_to_the_maximum_period()
    {
        Guid classId = GuidMother.Random();
        string inside = StringMother.Random("inside");
        this.ShouldListEventsOfClass(
            classId,
            CalendarEventResponseMother.OneTime(LAST_DAY_OF_MAXIMUM, inside),
            CalendarEventResponseMother.OneTime(LAST_DAY_OF_MAXIMUM.AddDays(ONE_DAY)));

        IReadOnlyCollection<CalendarEventUpcomingResponse> result = await this._useCase.ExecuteAsync(
            CalendarEventUpcomingFinderByClassQueryMother.WithDays(classId, SummaryPeriod.MaxDays),
            CancellationToken.None);

        result.Select(upcoming => upcoming.Title).Should().Equal(inside);
    }

    [Test]
    public async Task order_the_events_of_the_same_day_by_title()
    {
        Guid classId = GuidMother.Random();
        DateOnly day = TODAY.AddDays(TWO_DAYS);
        this.ShouldListEventsOfClass(
            classId,
            CalendarEventResponseMother.OneTime(day, "b"),
            CalendarEventResponseMother.OneTime(day, "a"),
            CalendarEventResponseMother.OneTime(day, "c"));

        IReadOnlyCollection<CalendarEventUpcomingResponse> result = await this.UpcomingAsync(classId);

        result.Select(upcoming => upcoming.Title).Should().Equal("a", "b", "c");
    }

    [Test]
    public async Task return_an_empty_list_when_the_class_has_no_events()
    {
        Guid classId = GuidMother.Random();
        this.ShouldListEventsOfClass(classId);

        IReadOnlyCollection<CalendarEventUpcomingResponse> result = await this.UpcomingAsync(classId);

        result.Should().BeEmpty();
    }

    [Test]
    public async Task take_today_from_the_utc_date_of_the_clock()
    {
        Guid classId = GuidMother.Random();
        string title = StringMother.Random("event");
        DateOnly day = TODAY.AddDays(TWO_DAYS);
        this.ShouldListEventsOfClass(
            classId,
            CalendarEventResponseMother.OneTime(TODAY),
            CalendarEventResponseMother.OneTime(day, title));
        this.AdvanceClock(TimeSpan.FromDays(ONE_DAY));

        IReadOnlyCollection<CalendarEventUpcomingResponse> result = await this.UpcomingAsync(classId);

        result.Should().Equal(
            CalendarEventUpcomingResponseMother.With(nameof(EventType.Meeting), day, TWO_DAYS - ONE_DAY, title));
    }

    [TestCase(SummaryPeriod.MinDays - ONE_DAY)]
    [TestCase(-REQUESTED_DAYS)]
    [TestCase(SummaryPeriod.MaxDays + ONE_DAY)]
    public async Task throw_invalid_days_without_asking_for_the_events_when_the_period_is_out_of_range(int days)
    {
        Func<Task> act = () => this._useCase.ExecuteAsync(
            CalendarEventUpcomingFinderByClassQueryMother.WithDays(GuidMother.Random(), days),
            CancellationToken.None);

        await act.Should().ThrowAsync<SummaryPeriodExceptions.InvalidDays>();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ask_for_the_events_of_the_requested_class_only()
    {
        Guid classId = GuidMother.Random();
        this.ShouldListEventsOfClass(classId);

        await this.UpcomingAsync(classId);

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

        Func<Task> act = () => this.UpcomingAsync(GuidMother.Random());

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

        Func<Task> act = () => this.UpcomingAsync(GuidMother.Random());

        await act.Should().ThrowAsync<ClassExceptions.NotFound>();
    }

    private Task<IReadOnlyCollection<CalendarEventUpcomingResponse>> UpcomingAsync(Guid classId)
        => this._useCase.ExecuteAsync(
            CalendarEventUpcomingFinderByClassQueryMother.WithClassId(classId),
            CancellationToken.None);

    private void ShouldListEventsOfClass(Guid classId, params CalendarEventResponse[] events)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<CalendarEventFinderByClass.Query>(query => query.ClassId == classId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<CalendarEventResponse>)events);
}
