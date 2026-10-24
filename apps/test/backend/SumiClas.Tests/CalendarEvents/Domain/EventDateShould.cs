namespace SumiClas.Tests.CalendarEvents.Domain;

using AwesomeAssertions;
using SumiClas.CalendarEvents.Domain;

[TestFixture]
public class EventDateShould
{
    private const int FEBRUARY = 2;
    private const int LEAP_DAY = 29;
    private const int LAST_DAY_OF_A_COMMON_FEBRUARY = 28;
    private const int OCTOBER = 10;
    private const int TENTH = 10;
    private const int JANUARY = 1;
    private const int DECEMBER = 12;
    private const int CHRISTMAS_EVE = 24;

    private static readonly DateOnly TODAY = new(2026, OCTOBER, TENTH);
    private static readonly DateOnly YESTERDAY = TODAY.AddDays(-1);
    private static readonly DateOnly TOMORROW = TODAY.AddDays(1);
    private static readonly DateOnly FEBRUARY_28_OF_A_COMMON_YEAR = new(2027, FEBRUARY, LAST_DAY_OF_A_COMMON_FEBRUARY);
    private static readonly DateOnly MARCH_1_OF_A_COMMON_YEAR = FEBRUARY_28_OF_A_COMMON_YEAR.AddDays(1);
    private static readonly DateOnly FEBRUARY_29_OF_A_LEAP_YEAR = new(2028, FEBRUARY, LEAP_DAY);

    [Test]
    public void return_the_date_of_a_one_time_event_that_is_today()
    {
        DateOnly? next = EventDateMother.On(TODAY).NextOccurrenceFrom(TODAY);

        next.Should().Be(TODAY);
    }

    [Test]
    public void return_the_date_of_a_one_time_event_that_is_ahead()
    {
        DateOnly? next = EventDateMother.On(TOMORROW).NextOccurrenceFrom(TODAY);

        next.Should().Be(TOMORROW);
    }

    [Test]
    public void return_nothing_for_a_one_time_event_that_is_already_over()
    {
        DateOnly? next = EventDateMother.On(YESTERDAY).NextOccurrenceFrom(TODAY);

        next.Should().BeNull();
    }

    [Test]
    public void return_this_year_for_a_recurring_event_that_has_not_happened_yet_this_year()
    {
        DateOnly? next = EventDateMother.RecurringAnnual(DECEMBER, CHRISTMAS_EVE).NextOccurrenceFrom(TODAY);

        next.Should().Be(new DateOnly(TODAY.Year, DECEMBER, CHRISTMAS_EVE));
    }

    [Test]
    public void return_today_for_a_recurring_event_that_is_today()
    {
        DateOnly? next = EventDateMother.RecurringAnnual(TODAY.Month, TODAY.Day).NextOccurrenceFrom(TODAY);

        next.Should().Be(TODAY);
    }

    [Test]
    public void return_next_year_for_a_recurring_event_that_already_happened_this_year()
    {
        DateOnly? next = EventDateMother.RecurringAnnual(JANUARY, TENTH).NextOccurrenceFrom(TODAY);

        next.Should().Be(new DateOnly(TODAY.Year + 1, JANUARY, TENTH));
    }

    [Test]
    public void return_next_year_for_a_recurring_event_that_was_yesterday()
    {
        DateOnly? next = EventDateMother.RecurringAnnual(YESTERDAY.Month, YESTERDAY.Day).NextOccurrenceFrom(TODAY);

        next.Should().Be(new DateOnly(TODAY.Year + 1, YESTERDAY.Month, YESTERDAY.Day));
    }

    [Test]
    public void fall_on_february_28_a_recurring_february_29_in_a_year_that_is_not_a_leap_year()
    {
        DateOnly? next = EventDateMother.RecurringAnnual(FEBRUARY, LEAP_DAY)
            .NextOccurrenceFrom(FEBRUARY_28_OF_A_COMMON_YEAR);

        next.Should().Be(FEBRUARY_28_OF_A_COMMON_YEAR);
    }

    [Test]
    public void keep_february_29_in_a_leap_year_after_the_common_one_has_gone_by()
    {
        DateOnly? next = EventDateMother.RecurringAnnual(FEBRUARY, LEAP_DAY)
            .NextOccurrenceFrom(MARCH_1_OF_A_COMMON_YEAR);

        next.Should().Be(FEBRUARY_29_OF_A_LEAP_YEAR);
    }

    [Test]
    public void keep_february_29_when_the_year_is_a_leap_year()
    {
        DateOnly? next = EventDateMother.RecurringAnnual(FEBRUARY, LEAP_DAY)
            .NextOccurrenceFrom(FEBRUARY_29_OF_A_LEAP_YEAR);

        next.Should().Be(FEBRUARY_29_OF_A_LEAP_YEAR);
    }
}
