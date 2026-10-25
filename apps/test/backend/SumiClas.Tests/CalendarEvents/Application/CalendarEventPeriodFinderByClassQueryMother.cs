namespace SumiClas.Tests.CalendarEvents.Application;

using SumiClas.CalendarEvents.Application;

public static class CalendarEventPeriodFinderByClassQueryMother
{
    public static CalendarEventPeriodFinderByClass.Query With(Guid classId, DateOnly from, DateOnly to)
        => new(classId, from, to);
}
