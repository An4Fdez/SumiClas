namespace SumiClas.CalendarEvents.Domain;

public interface ICalendarEventRepository
{
    Task<CalendarEvent?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(CalendarEvent calendarEvent, CancellationToken ct = default);
}
