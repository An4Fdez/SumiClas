using MediatR;
using SumiClas.Shared.Application;
using SumiClas.CalendarEvents.Domain;

namespace SumiClas.CalendarEvents.Application;

public static class CalendarEventFinderByIds
{
    /// <summary>Returns only the events that exist.</summary>
    public sealed record Query(IReadOnlyCollection<Guid> CalendarEventIds) : IQuery<IReadOnlyCollection<CalendarEventSummaryResponse>>;

    public sealed class Finder(ICalendarEventRepository repository) : IUseCase
    {
        public async Task<IReadOnlyCollection<CalendarEventSummaryResponse>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            IReadOnlyCollection<CalendarEvent> events = await repository.SearchAsync(new CalendarEventSpecification(ids: query.CalendarEventIds), ct);
            return events.Select(calendarEvent => new CalendarEventSummaryResponse(calendarEvent.Id, calendarEvent.Title)).ToList();
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, IReadOnlyCollection<CalendarEventSummaryResponse>>
    {
        public Task<IReadOnlyCollection<CalendarEventSummaryResponse>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
