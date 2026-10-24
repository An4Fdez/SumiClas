using SumiClas.Shared.Domain;

namespace SumiClas.WeeklySummaries.Domain;

public sealed class WeeklySummarySpecification : Specification<WeeklySummary>
{
    public WeeklySummarySpecification(
        Guid? id = null,
        Guid? classId = null,
        DateOnly? weekStart = null,
        string? content = null,
        string? inputHash = null,
        DateTimeOffset? generatedAt = null)
    {
        if (id is { } summaryId) AddCriteria(summary => summary.Id == summaryId);
        if (classId is { } schoolClass) AddCriteria(summary => summary.ClassId == schoolClass);
        if (weekStart is { } week) AddCriteria(summary => summary.WeekStart == week);
        if (content is not null) AddCriteria(summary => summary.Content == content);
        if (inputHash is not null) AddCriteria(summary => summary.InputHash == inputHash);
        if (generatedAt is { } generated) AddCriteria(summary => summary.GeneratedAt == generated);
    }
}
