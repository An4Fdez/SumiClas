using SumiClas.Shared.Domain;

namespace SumiClas.CalendarEvents.Domain;

/// <summary>
/// A one-time date (Year set) for regular events, or a recurring annual month+day
/// (Year null) for birthdays — never a real year for those, per the privacy rule in
/// the domain doc (section 7.3): it must be structurally impossible to derive a
/// child's exact age from a stored birth year.
/// </summary>
public sealed class EventDate : ValueObject
{
    public int Month { get; }
    public int Day { get; }
    public int? Year { get; }

    public bool IsRecurringAnnual => Year is null;

    private EventDate(int month, int day, int? year)
    {
        Month = month;
        Day = day;
        Year = year;
    }

    public static EventDate OnDate(DateOnly date)
    {
        return new EventDate(date.Month, date.Day, date.Year);
    }

    public static EventDate RecurringAnnual(int month, int day)
    {
        Validate(month, day);

        return new EventDate(month, day, null);
    }

    /// <summary>
    /// The first date on or after <paramref name="from"/> on which the event happens, or null when a one-time event is
    /// already over. A recurring Feb 29 falls on Feb 28 in the years that are not leap years.
    /// </summary>
    public DateOnly? NextOccurrenceFrom(DateOnly from)
    {
        if (Year is { } year)
        {
            DateOnly date = new(year, Month, Day);
            return date >= from ? date : null;
        }

        DateOnly thisYear = OccurrenceIn(from.Year);
        return thisYear >= from ? thisYear : OccurrenceIn(from.Year + 1);
    }

    private DateOnly OccurrenceIn(int year) => new(year, Month, Math.Min(Day, DateTime.DaysInMonth(year, Month)));

    private static void Validate(int month, int day)
    {
        if (month < 1 || month > 12)
        {
            throw new CalendarEventExceptions.InvalidMonth();
        }

        // 2000 is a leap year, so Feb 29 is always accepted as a valid recurring day.
        if (day < 1 || day > DateTime.DaysInMonth(2000, month))
        {
            throw new CalendarEventExceptions.InvalidDay();
        }
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Month;
        yield return Day;
        yield return Year;
    }
}
