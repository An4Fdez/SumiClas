using SumiClas.Shared.Domain;

namespace SumiClas.SchoolYears.Domain;

public interface ISchoolYearRepository
{
    Task<SchoolYear?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<SchoolYear>> SearchAsync(ISpecification<SchoolYear> specification, CancellationToken ct = default);
    Task AddAsync(SchoolYear schoolYear, CancellationToken ct = default);
}
