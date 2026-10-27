using MediatR;
using SumiClas.Announcements.Application;
using SumiClas.Assignments.Application;
using SumiClas.CalendarEvents.Application;
using SumiClas.ClassExpenses.Application;
using SumiClas.LostItems.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.Dashboard.Application;

public static class DashboardSummaryFinderByClass
{
    /// <summary>
    /// The numbers the dashboard shows for the current user in the class, gathered from the modules that own them.
    /// <c>Days</c> (<see cref="SummaryPeriod.DefaultDays"/> when not given) sets the period of the announcements, the
    /// lost items and the upcoming events. If any of them fails, the whole summary fails.
    /// </summary>
    public sealed record Query(Guid ClassId, int? Days = null) : IQuery<DashboardSummaryResponse>;

    public sealed class Finder(IQueryBus queryBus) : IUseCase
    {
        public async Task<DashboardSummaryResponse> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            int days = SummaryPeriod.Resolve(query.Days);

            // One after another: the queries share the request's database context, which cannot run two at once.
            AnnouncementUnreadSummaryResponse announcements = await queryBus.Send(
                new AnnouncementUnreadSummaryFinderByClass.Query(query.ClassId, days), ct);
            AssignmentPendingSummaryResponse assignments = await queryBus.Send(
                new AssignmentPendingSummaryFinderByClass.Query(query.ClassId), ct);
            ClassExpensePendingSummaryResponse expenses = await queryBus.Send(
                new ClassExpensePendingSummaryFinderByClass.Query(query.ClassId), ct);
            LostItemRecentSummaryResponse lostItems = await queryBus.Send(
                new LostItemRecentSummaryFinderByClass.Query(query.ClassId, days), ct);
            LostItemClaimPendingSummaryResponse lostItemClaims = await queryBus.Send(
                new LostItemClaimPendingSummaryFinderByClass.Query(query.ClassId), ct);
            IReadOnlyCollection<CalendarEventUpcomingResponse> upcomingEvents = await queryBus.Send(
                new CalendarEventUpcomingFinderByClass.Query(query.ClassId, days), ct);

            return new DashboardSummaryResponse(days, announcements, assignments, expenses, lostItems, lostItemClaims, upcomingEvents);
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, DashboardSummaryResponse>
    {
        public Task<DashboardSummaryResponse> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
