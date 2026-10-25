using MediatR;
using SumiClas.Shared.Application;

namespace SumiClas.CalendarEvents.Application;

public static class CalendarEventPeriodFinderByClass
{
    /// <summary>
    /// The events the class has from the day <c>From</c> to the day <c>To</c>, both included, soonest first. They are
    /// the ones <see cref="CalendarEventFinderByClass"/> lists (school-wide, of the class and the birthdays of its
    /// students); each one appears once, on its first date inside the period. A period that ends before it starts has
    /// none.
    /// </summary>
    public sealed record Query(Guid ClassId, DateOnly From, DateOnly To)
        : IQuery<IReadOnlyCollection<CalendarEventPeriodResponse>>;

    public sealed class Finder(IQueryBus queryBus) : IUseCase
    {
        public async Task<IReadOnlyCollection<CalendarEventPeriodResponse>> ExecuteAsync(
            Query query,
            CancellationToken ct = default)
        {
            IReadOnlyCollection<CalendarEventResponse> events =
                await queryBus.Send(new CalendarEventFinderByClass.Query(query.ClassId), ct);

            return events
                .Select(calendarEvent => (Event: calendarEvent, Date: calendarEvent.ToEventDate().NextOccurrenceFrom(query.From)))
                .Where(candidate => candidate.Date is { } date && date <= query.To)
                .Select(candidate => new CalendarEventPeriodResponse(
                    candidate.Event.Title,
                    candidate.Event.Type,
                    candidate.Date!.Value))
                .OrderBy(inPeriod => inPeriod.Date)
                .ThenBy(inPeriod => inPeriod.Title, StringComparer.Ordinal)
                .ToList();
        }
    }

    public sealed class Handler(Finder finder)
        : IRequestHandler<Query, IReadOnlyCollection<CalendarEventPeriodResponse>>
    {
        public Task<IReadOnlyCollection<CalendarEventPeriodResponse>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
