namespace SumiClas.Tests.CalendarEvents.Application;

using SumiClas.CalendarEvents.Application;
using SumiClas.Tests.Shared;

public static class CalendarEventCreateCommandMother
{
    private static readonly DateOnly SomeDay = DateOnly.FromDateTime(UnitTestCase.Now.UtcDateTime).AddDays(30);

    public static CalendarEventCreate.Command With(
        Guid id,
        string scope,
        Guid? classId,
        string type,
        string title,
        DateOnly date,
        TimeOnly? time,
        string? location,
        string? cost)
        => new(id, scope, classId, type, title, date, time, location, cost);

    public static CalendarEventCreate.Command Valid(
        Guid? id = null,
        string? scope = null,
        Guid? classId = null,
        string? type = null,
        string? title = null)
        => With(
            id ?? GuidMother.Random(),
            scope ?? "School",
            classId ?? GuidMother.Random(),
            type ?? "Trip",
            title ?? StringMother.Random("Event"),
            SomeDay,
            null,
            null,
            null);

    public static CalendarEventCreate.Command ClassScopeWithClassId(
        Guid? classId,
        string? type = null,
        string? title = null)
        => With(
            GuidMother.Random(),
            "Class",
            classId,
            type ?? "Trip",
            title ?? StringMother.Random("Event"),
            SomeDay,
            null,
            null,
            null);
}
