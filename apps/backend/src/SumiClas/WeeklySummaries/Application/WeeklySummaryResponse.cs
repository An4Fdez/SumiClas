namespace SumiClas.WeeklySummaries.Application;

/// <summary>The summary of the current week of a class.</summary>
/// <param name="WeekStart">The Monday the week starts on.</param>
/// <param name="WeekEnd">The Sunday the week ends on.</param>
/// <param name="Content">The text, or null when the week has no news to tell.</param>
/// <param name="GeneratedAt">When the text was written, or null when there is none.</param>
public sealed record WeeklySummaryResponse(
    DateOnly WeekStart,
    DateOnly WeekEnd,
    string? Content,
    DateTimeOffset? GeneratedAt);
