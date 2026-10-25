namespace SumiClas.Tests.CalendarEvents.Application;

using SumiClas.CalendarEvents.Application;
using SumiClas.Tests.Shared;

public static class CalendarEventUpcomingResponseMother
{
    public static CalendarEventUpcomingResponse With(string type, DateOnly date, int daysUntil, string? title = null)
        => new(title ?? StringMother.Random("event"), type, date, daysUntil);
}
