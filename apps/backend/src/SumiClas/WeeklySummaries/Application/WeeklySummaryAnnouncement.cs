namespace SumiClas.WeeklySummaries.Application;

/// <summary>An announcement of the week as the writer sees it: its title and whether it is urgent, never its body or its author.</summary>
public sealed record WeeklySummaryAnnouncement(string Title, bool Urgent);
