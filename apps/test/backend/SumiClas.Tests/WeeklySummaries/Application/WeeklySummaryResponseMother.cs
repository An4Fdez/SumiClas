namespace SumiClas.Tests.WeeklySummaries.Application;

using SumiClas.WeeklySummaries.Application;

public static class WeeklySummaryResponseMother
{
    public static WeeklySummaryResponse Written(
        DateOnly weekStart,
        DateOnly weekEnd,
        string content,
        DateTimeOffset generatedAt)
        => new(weekStart, weekEnd, content, generatedAt);

    public static WeeklySummaryResponse WithoutNews(DateOnly weekStart, DateOnly weekEnd)
        => new(weekStart, weekEnd, Content: null, GeneratedAt: null);
}
