namespace SumiClas.WeeklySummaries.Domain;

/// <summary>A non-birthday event of the week as the writer sees it: a birthday title names a child, so birthdays are only counted.</summary>
public sealed record WeeklySummaryEvent(string Title, DateOnly Date);
