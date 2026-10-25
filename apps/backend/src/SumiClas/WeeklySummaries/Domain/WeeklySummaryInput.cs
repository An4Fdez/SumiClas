namespace SumiClas.WeeklySummaries.Domain;

/// <summary>
/// Everything the writer may know about a class and a week. It has no field for a person (no student, guardian, author,
/// participant, email or comment), so what leaves for a third party can only be class-level titles, dates and counts.
/// </summary>
public sealed record WeeklySummaryInput(
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    IReadOnlyCollection<WeeklySummaryAnnouncement> Announcements,
    IReadOnlyCollection<WeeklySummaryEvent> Events,
    IReadOnlyCollection<WeeklySummaryAssignment> Assignments,
    IReadOnlyCollection<string> PendingExpenses,
    int Birthdays,
    int UnrecoveredLostItems)
{
    public bool IsEmpty
        => Announcements.Count == 0
            && Events.Count == 0
            && Assignments.Count == 0
            && PendingExpenses.Count == 0
            && Birthdays <= 0
            && UnrecoveredLostItems <= 0;
}
