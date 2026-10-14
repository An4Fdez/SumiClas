namespace SumiClas.Tests.CalendarEvents.Domain;

using SumiClas.CalendarEvents.Domain;

public static class EventDateMother
{
    public static EventDate On(DateOnly date) => EventDate.OnDate(date).Value;

    public static EventDate RecurringAnnual(int month, int day) => EventDate.RecurringAnnual(month, day).Value;
}
