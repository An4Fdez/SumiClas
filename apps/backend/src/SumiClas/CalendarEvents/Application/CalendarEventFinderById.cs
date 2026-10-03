using MediatR;
using SumiClas.CalendarEvents.Domain.Services;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.CalendarEvents.Application;

public static class CalendarEventFinderById
{
    public sealed record Query(Guid CalendarEventId) : IQuery<Result<CalendarEventResponse>>;

    /// <summary>Deliberately minimal: other modules only need to know the event exists.</summary>
    public sealed record CalendarEventResponse(Guid Id, string Title);

    public sealed class Handler(CalendarEventByIdFinder finder) : IRequestHandler<Query, Result<CalendarEventResponse>>
    {
        public async Task<Result<CalendarEventResponse>> Handle(Query query, CancellationToken ct)
        {
            var found = await finder.FindAsync(query.CalendarEventId, ct);
            return found.IsFailure
                ? Result<CalendarEventResponse>.Failure(found.Error!)
                : Result<CalendarEventResponse>.Success(new CalendarEventResponse(found.Value.Id, found.Value.Title));
        }
    }
}
