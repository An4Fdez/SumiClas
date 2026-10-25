namespace SumiClas.Tests.WeeklySummaries.Domain;

using SumiClas.Tests.Shared;
using SumiClas.WeeklySummaries.Domain;

public static class WeeklySummaryInputMother
{
    private const int DAYS_IN_A_WEEK = 7;

    private static readonly DateOnly WeekStart = DateOnly.FromDateTime(UnitTestCase.Now.UtcDateTime);

    public static WeeklySummaryInput Empty() => With();

    public static WeeklySummaryInput With(
        IReadOnlyCollection<WeeklySummaryAnnouncement>? announcements = null,
        IReadOnlyCollection<WeeklySummaryEvent>? events = null,
        IReadOnlyCollection<WeeklySummaryAssignment>? assignments = null,
        IReadOnlyCollection<string>? pendingExpenses = null,
        int birthdays = 0,
        int unrecoveredLostItems = 0)
        => new(
            WeekStart,
            WeekStart.AddDays(DAYS_IN_A_WEEK - 1),
            announcements ?? [],
            events ?? [],
            assignments ?? [],
            pendingExpenses ?? [],
            birthdays,
            unrecoveredLostItems);

    public static DateOnly Day(int daysFromStart) => WeekStart.AddDays(daysFromStart);

    public static WeeklySummaryAnnouncement Announcement(string? title = null, bool urgent = false)
        => new(title ?? StringMother.Random("announcement"), urgent);

    public static WeeklySummaryEvent Event(int daysFromStart, string? title = null)
        => new(title ?? StringMother.Random("event"), Day(daysFromStart));

    public static WeeklySummaryAssignment Assignment(int daysFromStart, string? title = null)
        => new(title ?? StringMother.Random("assignment"), Day(daysFromStart));
}
