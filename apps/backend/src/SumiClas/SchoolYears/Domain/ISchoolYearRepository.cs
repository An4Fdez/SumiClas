namespace SumiClas.SchoolYears.Domain;

public interface ISchoolYearRepository
{
    Task<SchoolYear?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<SchoolYear?> GetActiveBySchoolIdAsync(Guid schoolId, CancellationToken ct = default);
    Task AddAsync(SchoolYear schoolYear, CancellationToken ct = default);
}
