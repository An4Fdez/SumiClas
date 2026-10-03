using SumiClas.Shared.Domain;

namespace SumiClas.CalendarEvents.Domain.Services;

public sealed class CalendarEventByIdFinder(ICalendarEventRepository calendarEvents)
{
    public async Task<Result<CalendarEvent>> FindAsync(Guid id, CancellationToken ct = default)
    {
        var calendarEvent = await calendarEvents.GetByIdAsync(id, ct);
        return calendarEvent is null
            ? Result<CalendarEvent>.Failure(CalendarEventErrors.NotFound)
            : Result<CalendarEvent>.Success(calendarEvent);
    }
}
