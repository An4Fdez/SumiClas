namespace SumiClas.Tests.Dashboard.Application;

using SumiClas.Dashboard.Application;

public static class DashboardSummaryResponseMother
{
    public static DashboardSummaryResponse With(int days)
        => new(
            days,
            DashboardCountersMother.Announcements(),
            DashboardCountersMother.Assignments(),
            DashboardCountersMother.Expenses(),
            DashboardCountersMother.LostItems(),
            DashboardCountersMother.UpcomingEvents());
}
