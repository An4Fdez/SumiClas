namespace SumiClas.Tests.CalendarEvents.Application;

using SumiClas.CalendarEvents.Application;
using SumiClas.CalendarEvents.Domain;
using SumiClas.Tests.Shared;

public static class CalendarEventResponseMother
{
    public static CalendarEventResponse OneTime(DateOnly date, string? title = null)
        => new(
            GuidMother.Random(),
            EventScope.Class.ToString(),
            GuidMother.Random(),
            null,
            EventType.Meeting.ToString(),
            title ?? StringMother.Random("event"),
            date.Month,
            date.Day,
            date.Year,
            null,
            null,
            null,
            GuidMother.Random(),
            StringMother.Random("author"),
            null,
            UnitTestCase.Now,
            null,
            false);

    public static CalendarEventResponse Birthday(int month, int day, string? title = null)
        => new(
            GuidMother.Random(),
            EventScope.Class.ToString(),
            null,
            GuidMother.Random(),
            EventType.Birthday.ToString(),
            title ?? StringMother.Random("birthday"),
            month,
            day,
            null,
            null,
            null,
            null,
            GuidMother.Random(),
            StringMother.Random("author"),
            null,
            UnitTestCase.Now,
            null,
            false);
}
