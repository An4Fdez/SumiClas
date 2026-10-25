namespace SumiClas.Tests.CalendarEvents.Application;

using SumiClas.CalendarEvents.Application;
using SumiClas.Tests.Shared;

public static class CalendarEventPeriodResponseMother
{
    public static CalendarEventPeriodResponse With(string type, DateOnly date, string? title = null)
        => new(title ?? StringMother.Random("event"), type, date);
}
