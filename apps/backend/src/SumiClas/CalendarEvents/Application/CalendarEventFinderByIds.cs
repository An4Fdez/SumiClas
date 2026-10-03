using MediatR;
using SumiClas.Shared.Application;
using SumiClas.CalendarEvents.Domain;

namespace SumiClas.CalendarEvents.Application;

public static class CalendarEventFinderByIds
{
    /// <summary>Returns only the events that exist.</summary>
    public sealed record Query(IReadOnlyCollection<Guid> CalendarEventIds) : IQuery<IReadOnlyCollection<CalendarEventSummaryResponse>>;

    public sealed class Handler(ICalendarEventRepository calendarEvents)
        : IRequestHandler<Query, IReadOnlyCollection<CalendarEventSummaryResponse>>
    {
        public async Task<IReadOnlyCollection<CalendarEventSummaryResponse>> Handle(Query query, CancellationToken ct)
        {
            var events = await calendarEvents.SearchAsync(new CalendarEventSpecification(ids: query.CalendarEventIds), ct);
            return events.Select(calendarEvent => new CalendarEventSummaryResponse(calendarEvent.Id, calendarEvent.Title)).ToList();
        }
    }
}
