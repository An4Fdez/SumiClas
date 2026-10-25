namespace SumiClas.CalendarEvents.Application;

/// <param name="Type">The event type, as in <see cref="CalendarEventResponse"/>.</param>
/// <param name="Date">The date it happens on inside the period; a birthday carries the one that falls in it.</param>
public sealed record CalendarEventPeriodResponse(string Title, string Type, DateOnly Date);
