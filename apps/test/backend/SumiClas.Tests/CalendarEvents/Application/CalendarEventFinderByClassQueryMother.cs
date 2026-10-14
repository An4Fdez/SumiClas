namespace SumiClas.Tests.CalendarEvents.Application;

using SumiClas.CalendarEvents.Application;

public static class CalendarEventFinderByClassQueryMother
{
    public static CalendarEventFinderByClass.Query WithClassId(Guid classId) => new(classId);
}
