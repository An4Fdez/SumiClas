namespace SumiClas.Tests.CalendarEvents.Application;

using SumiClas.CalendarEvents.Application;

public static class CalendarEventFinderByIdQueryMother
{
    public static CalendarEventFinderById.Query WithId(Guid calendarEventId) => new(calendarEventId);
}
