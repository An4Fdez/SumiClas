using SumiClas.Shared.Domain;

namespace SumiClas.WeeklySummaries.Domain;

public interface IWeeklySummaryRepository
{
    Task<WeeklySummary?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<WeeklySummary>> SearchAsync(ISpecification<WeeklySummary> specification, CancellationToken ct = default);
    Task AddAsync(WeeklySummary weeklySummary, CancellationToken ct = default);
}
