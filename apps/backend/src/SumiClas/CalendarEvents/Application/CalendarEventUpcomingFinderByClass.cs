using MediatR;
using SumiClas.CalendarEvents.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.CalendarEvents.Application;

public static class CalendarEventUpcomingFinderByClass
{
    /// <summary>
    /// The events the class has from today until <c>Days</c> days ahead (<see cref="SummaryPeriod.DefaultDays"/> when
    /// not given), both days included, soonest first. They are the ones <see cref="CalendarEventFinderByClass"/> lists
    /// (school-wide, of the class and the birthdays of its students). "Today" is the UTC date of the clock.
    /// </summary>
    public sealed record Query(Guid ClassId, int? Days = null)
        : IQuery<IReadOnlyCollection<CalendarEventUpcomingResponse>>;

    public sealed class Finder(IQueryBus queryBus, IClock clock) : IUseCase
    {
        public async Task<IReadOnlyCollection<CalendarEventUpcomingResponse>> ExecuteAsync(
            Query query,
            CancellationToken ct = default)
        {
            int days = SummaryPeriod.Resolve(query.Days);

            IReadOnlyCollection<CalendarEventResponse> events =
                await queryBus.Send(new CalendarEventFinderByClass.Query(query.ClassId), ct);

            DateOnly today = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
            DateOnly last = today.AddDays(days);

            return events
                .Select(calendarEvent => (Event: calendarEvent, Date: calendarEvent.ToEventDate().NextOccurrenceFrom(today)))
                .Where(candidate => candidate.Date is { } date && date <= last)
                .Select(candidate => new CalendarEventUpcomingResponse(
                    candidate.Event.Title,
                    candidate.Event.Type,
                    candidate.Date!.Value,
                    candidate.Date.Value.DayNumber - today.DayNumber))
                .OrderBy(upcoming => upcoming.Date)
                .ThenBy(upcoming => upcoming.Title, StringComparer.Ordinal)
                .ToList();
        }
    }

    public sealed class Handler(Finder finder)
        : IRequestHandler<Query, IReadOnlyCollection<CalendarEventUpcomingResponse>>
    {
        public Task<IReadOnlyCollection<CalendarEventUpcomingResponse>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
