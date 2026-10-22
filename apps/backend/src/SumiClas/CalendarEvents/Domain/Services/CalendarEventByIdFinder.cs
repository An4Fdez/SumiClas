namespace SumiClas.CalendarEvents.Domain.Services;

public sealed class CalendarEventByIdFinder(ICalendarEventRepository calendarEvents)
{
    public async Task<CalendarEvent> FindAsync(Guid id, CancellationToken ct = default)
        => await calendarEvents.GetByIdAsync(id, ct) ?? throw new CalendarEventExceptions.NotFound();
}
