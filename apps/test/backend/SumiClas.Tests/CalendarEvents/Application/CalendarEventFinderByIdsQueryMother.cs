namespace SumiClas.Tests.CalendarEvents.Application;

using SumiClas.CalendarEvents.Application;

public static class CalendarEventFinderByIdsQueryMother
{
    public static CalendarEventFinderByIds.Query WithIds(params Guid[] calendarEventIds) => new(calendarEventIds);
}
