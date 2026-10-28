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
            GuidMother.Random(),
            null,
            EventType.Other,
            title,
            EventDateMother.On(SomeDay),
            null,
            null,
            null,
            GuidMother.Random(),
            UnitTestCase.Now,
            null);

    public static CalendarEvent SchoolWide(Guid schoolId, string title, DateOnly date, Guid authorId)
        => new(
            GuidMother.Random(),
            EventScope.School,
            null,
            schoolId,
            null,
            EventType.Activity,
            title,
            EventDateMother.On(date),
            null,
            null,
            null,
            authorId,
            UnitTestCase.Now,
            null);

    public static CalendarEvent ForClass(Guid classId, string title, DateOnly date, Guid authorId)
        => new(
            GuidMother.Random(),
            EventScope.Class,
            classId,
            null,
            null,
            EventType.Meeting,
            title,
            EventDateMother.On(date),
            null,
            null,
            null,
            authorId,
            UnitTestCase.Now,
            null);

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
            null,
            EventType.Trip,
            title,
            EventDateMother.On(date),
            time,
            location,
            cost,
            authorId,
            createdAt,
            null);

    public static CalendarEvent Birthday(Guid studentId, string title, int month, int day, Guid authorId)
        => new(
            GuidMother.Random(),
            EventScope.Class,
            null,
            null,
            studentId,
            EventType.Birthday,
            title,
            EventDateMother.RecurringAnnual(month, day),
            null,
            null,
            null,
            authorId,
            UnitTestCase.Now,
            null);
}
