namespace SumiClas.CalendarEvents.Application;

/// <param name="Type">The event type, as in <see cref="CalendarEventResponse"/>.</param>
/// <param name="Date">The date it happens on, from today on; a birthday carries its next one.</param>
/// <param name="DaysUntil">Whole days from today to <paramref name="Date"/> (0 when it is today).</param>
public sealed record CalendarEventUpcomingResponse(string Title, string Type, DateOnly Date, int DaysUntil);
