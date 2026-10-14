namespace SumiClas.Tests.CalendarEvents.Application;

using SumiClas.CalendarEvents.Application;

public static class CalendarEventSummaryResponseMother
{
    public static CalendarEventSummaryResponse With(Guid id, string title) => new(id, title);
}
