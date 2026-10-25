using SumiClas.CalendarEvents.Domain;

namespace SumiClas.CalendarEvents.Application;

public static class CalendarEventResponseExtensions
{
    /// <summary>The date of the event: a real day when it has a year, the same day every year when it does not.</summary>
    public static EventDate ToEventDate(this CalendarEventResponse calendarEvent)
        => calendarEvent.Year is { } year
            ? EventDate.OnDate(new DateOnly(year, calendarEvent.Month, calendarEvent.Day))
            : EventDate.RecurringAnnual(calendarEvent.Month, calendarEvent.Day);
}
