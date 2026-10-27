using SumiClas.Announcements.Application;
using SumiClas.Assignments.Application;
using SumiClas.CalendarEvents.Application;
using SumiClas.ClassExpenses.Application;
using SumiClas.LostItems.Application;

namespace SumiClas.Dashboard.Application;

/// <param name="Days">The period, in days, that the backward-looking counters and the upcoming events cover.</param>
/// <param name="LostItemClaims">Claims waiting for the user to accept or reject, on their own open lost items.</param>
/// <param name="UpcomingEvents">Soonest first, so the first one is the next event of the class.</param>
public sealed record DashboardSummaryResponse(
    int Days,
    AnnouncementUnreadSummaryResponse Announcements,
    AssignmentPendingSummaryResponse Assignments,
    ClassExpensePendingSummaryResponse Expenses,
    LostItemRecentSummaryResponse LostItems,
    LostItemClaimPendingSummaryResponse LostItemClaims,
    IReadOnlyCollection<CalendarEventUpcomingResponse> UpcomingEvents);
