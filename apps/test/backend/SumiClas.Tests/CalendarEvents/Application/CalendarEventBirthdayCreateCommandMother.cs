namespace SumiClas.Tests.CalendarEvents.Application;

using SumiClas.CalendarEvents.Application;
using SumiClas.Tests.Shared;

public static class CalendarEventBirthdayCreateCommandMother
{
    public static CalendarEventBirthdayCreate.Command With(
        Guid id,
        Guid studentId,
        string title,
        int month,
        int day)
        => new(id, studentId, title, month, day);

    public static CalendarEventBirthdayCreate.Command Valid(
        Guid? id = null,
        Guid? studentId = null,
        string? title = null,
        int? month = null,
        int? day = null)
        => With(
            id ?? GuidMother.Random(),
            studentId ?? GuidMother.Random(),
            title ?? StringMother.Random("Birthday"),
            month ?? 5,
            day ?? 17);
}
