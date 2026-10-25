namespace SumiClas.WeeklySummaries.Domain;

/// <summary>A homework of the week as the writer sees it: its title and due date, never its comments or who did it.</summary>
public sealed record WeeklySummaryAssignment(string Title, DateOnly DueDate);
