using MediatR;
using SumiClas.CalendarEvents.Domain;
using SumiClas.CalendarEvents.Domain.Services;
using SumiClas.Shared.Application;

namespace SumiClas.CalendarEvents.Application;

public static class CalendarEventFinderById
{
    public sealed record Query(Guid CalendarEventId) : IQuery<CalendarEventSummaryResponse>;

    public sealed class Finder(CalendarEventByIdFinder finder) : IUseCase
    {
        public async Task<CalendarEventSummaryResponse> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            CalendarEvent found = await finder.FindAsync(query.CalendarEventId, ct);
            return new CalendarEventSummaryResponse(found.Id, found.Title);
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, CalendarEventSummaryResponse>
    {
        public Task<CalendarEventSummaryResponse> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
