namespace SumiClas.Tests.WeeklySummaries.Domain;

using SumiClas.WeeklySummaries.Domain;

public static class WeeklySummarySpecificationMother
{
    public static WeeklySummarySpecification Unfiltered() => new();

    public static WeeklySummarySpecification WithId(Guid id) => new(id: id);

    public static WeeklySummarySpecification OfClass(Guid classId) => new(classId: classId);

    public static WeeklySummarySpecification OfWeek(DateOnly weekStart) => new(weekStart: weekStart);

    public static WeeklySummarySpecification WithContent(string content) => new(content: content);

    public static WeeklySummarySpecification WithInputHash(string inputHash) => new(inputHash: inputHash);

    public static WeeklySummarySpecification GeneratedAt(DateTimeOffset moment) => new(generatedAt: moment);
}
