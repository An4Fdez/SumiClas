namespace SumiClas.Tests.Dashboard.Application;

using SumiClas.Announcements.Application;
using SumiClas.Assignments.Application;
using SumiClas.CalendarEvents.Application;
using SumiClas.ClassExpenses.Application;
using SumiClas.Dashboard.Application;
using SumiClas.LostItems.Application;

public static class DashboardSummaryResponseMother
{
    public static DashboardSummaryResponse With(int days)
        => With(
            days,
            DashboardCountersMother.Announcements(),
            DashboardCountersMother.Assignments(),
            DashboardCountersMother.Expenses(),
            DashboardCountersMother.LostItems(),
            DashboardCountersMother.UpcomingEvents());

    public static DashboardSummaryResponse With(
        int days,
        AnnouncementUnreadSummaryResponse announcements,
        AssignmentPendingSummaryResponse assignments,
        ClassExpensePendingSummaryResponse expenses,
        LostItemRecentSummaryResponse lostItems,
        IReadOnlyCollection<CalendarEventUpcomingResponse> upcomingEvents)
        => new(days, announcements, assignments, expenses, lostItems, upcomingEvents);
}
