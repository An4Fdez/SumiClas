using MediatR;
using SumiClas.CalendarEvents.Domain.Services;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.CalendarEvents.Application;

public static class CalendarEventFinderById
{
    public sealed record Query(Guid CalendarEventId) : IQuery<Result<CalendarEventSummaryResponse>>;

    public sealed class Handler(CalendarEventByIdFinder finder) : IRequestHandler<Query, Result<CalendarEventSummaryResponse>>
    {
        public async Task<Result<CalendarEventSummaryResponse>> Handle(Query query, CancellationToken ct)
        {
            var found = await finder.FindAsync(query.CalendarEventId, ct);
            return found.IsFailure
                ? Result<CalendarEventSummaryResponse>.Failure(found.Error!)
                : Result<CalendarEventSummaryResponse>.Success(new CalendarEventSummaryResponse(found.Value.Id, found.Value.Title));
        }
    }
}
