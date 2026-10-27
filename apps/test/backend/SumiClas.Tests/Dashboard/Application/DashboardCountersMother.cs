namespace SumiClas.Tests.Dashboard.Application;

using SumiClas.Announcements.Application;
using SumiClas.Assignments.Application;
using SumiClas.CalendarEvents.Application;
using SumiClas.ClassExpenses.Application;
using SumiClas.LostItems.Application;
using SumiClas.Tests.Shared;

public static class DashboardCountersMother
{
    private const int MAXIMUM_COUNT = 1000;

    public static AnnouncementUnreadSummaryResponse Announcements()
        => new(Random.Shared.Next(1, MAXIMUM_COUNT), Random.Shared.Next(1, MAXIMUM_COUNT));

    public static AssignmentPendingSummaryResponse Assignments()
        => new(Random.Shared.Next(1, MAXIMUM_COUNT), DateOnly.FromDateTime(UnitTestCase.Now.UtcDateTime));

    public static ClassExpensePendingSummaryResponse Expenses()
        => new(true, StringMother.Random("expense"));

    public static LostItemRecentSummaryResponse LostItems()
        => new(Random.Shared.Next(1, MAXIMUM_COUNT));

    public static LostItemClaimPendingSummaryResponse LostItemClaims()
        => new(Random.Shared.Next(1, MAXIMUM_COUNT));

    public static IReadOnlyCollection<CalendarEventUpcomingResponse> UpcomingEvents()
        =>
        [
            new CalendarEventUpcomingResponse(
                StringMother.Random("event"),
                "Meeting",
                DateOnly.FromDateTime(UnitTestCase.Now.UtcDateTime),
                0),
        ];
}
