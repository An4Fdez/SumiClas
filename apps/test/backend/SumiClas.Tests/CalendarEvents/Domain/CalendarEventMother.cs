namespace SumiClas.Tests.CalendarEvents.Domain;

using SumiClas.CalendarEvents.Domain;
using SumiClas.Tests.Shared;

public static class CalendarEventMother
{
    private static readonly DateOnly SomeDay = DateOnly.FromDateTime(UnitTestCase.Now.UtcDateTime).AddDays(30);

    public static CalendarEvent WithId(Guid id) => With(id, StringMother.Random("Event"));

    public static CalendarEvent With(Guid id, string title)
        => new(
            id,
            EventScope.School,
            null,
            null,
            EventType.Other,
            title,
            EventDateMother.On(SomeDay),
            null,
            null,
            null,
            GuidMother.Random(),
            UnitTestCase.Now);

    public static CalendarEvent SchoolWide(string title, DateOnly date, Guid authorId)
        => new(
            GuidMother.Random(),
            EventScope.School,
            null,
            null,
            EventType.Activity,
            title,
            EventDateMother.On(date),
            null,
            null,
            null,
            authorId,
            UnitTestCase.Now);

    public static CalendarEvent ForClass(Guid classId, string title, DateOnly date, Guid authorId)
        => new(
            GuidMother.Random(),
            EventScope.Class,
            classId,
            null,
            EventType.Meeting,
            title,
            EventDateMother.On(date),
            null,
            null,
            null,
            authorId,
            UnitTestCase.Now);

    public static CalendarEvent Trip(
        Guid classId,
        string title,
        DateOnly date,
        TimeOnly time,
        string location,
        string cost,
        Guid authorId,
        DateTimeOffset createdAt)
        => new(
            GuidMother.Random(),
            EventScope.Class,
            classId,
            null,
            EventType.Trip,
            title,
            EventDateMother.On(date),
            time,
            location,
            cost,
            authorId,
            createdAt);

    public static CalendarEvent Birthday(Guid studentId, string title, int month, int day, Guid authorId)
        => new(
            GuidMother.Random(),
            EventScope.Class,
            null,
            studentId,
            EventType.Birthday,
            title,
            EventDateMother.RecurringAnnual(month, day),
            null,
            null,
            null,
            authorId,
            UnitTestCase.Now);
}
