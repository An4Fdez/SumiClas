namespace SumiClas.CalendarEvents.Application;

/// <summary>Deliberately minimal: other modules only need to point at an event (id and title).</summary>
public sealed record CalendarEventSummaryResponse(Guid Id, string Title);
