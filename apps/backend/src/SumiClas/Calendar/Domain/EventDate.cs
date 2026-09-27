using SumiClas.Shared.Domain;

namespace SumiClas.Calendar.Domain;

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

    public static Result<EventDate> OnDate(DateOnly date)
    {
        return Result<EventDate>.Success(new EventDate(date.Month, date.Day, date.Year));
    }

    public static Result<EventDate> RecurringAnnual(int month, int day)
    {
        if (month < 1 || month > 12)
        {
            return Result<EventDate>.Failure(CalendarEventErrors.InvalidMonth);
        }

        // 2000 is a leap year, so Feb 29 is always accepted as a valid recurring day.
        if (day < 1 || day > DateTime.DaysInMonth(2000, month))
        {
            return Result<EventDate>.Failure(CalendarEventErrors.InvalidDay);
        }

        return Result<EventDate>.Success(new EventDate(month, day, null));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Month;
        yield return Day;
        yield return Year;
    }
}
