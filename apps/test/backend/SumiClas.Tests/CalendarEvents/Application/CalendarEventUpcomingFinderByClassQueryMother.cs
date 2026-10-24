namespace SumiClas.Tests.CalendarEvents.Application;

using SumiClas.CalendarEvents.Application;

public static class CalendarEventUpcomingFinderByClassQueryMother
{
    public static CalendarEventUpcomingFinderByClass.Query WithClassId(Guid classId) => new(classId);

    public static CalendarEventUpcomingFinderByClass.Query WithDays(Guid classId, int days) => new(classId, days);
}
