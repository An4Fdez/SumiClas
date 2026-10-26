using SumiClas.Shared.Domain;

namespace SumiClas.CalendarEvents.Domain;

public interface ICalendarEventRepository
{
    Task<CalendarEvent?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<CalendarEvent>> SearchAsync(ISpecification<CalendarEvent> specification, CancellationToken ct = default);
    Task AddAsync(CalendarEvent calendarEvent, CancellationToken ct = default);
    void Remove(CalendarEvent calendarEvent);
}
