namespace SumiClas.Tests.CalendarEvents.Application;

using SumiClas.CalendarEvents.Application;
using SumiClas.CalendarEvents.Domain;

public static class CalendarEventUpdateCommandMother
{
    public static CalendarEventUpdate.Command With(Guid id, string type, string title, DateOnly date)
        => new(id, type, title, date, null, null, null);

    public static CalendarEventUpdate.Command Blank(Guid id, DateOnly date)
        => With(id, nameof(EventType.Other), " ", date);
}
