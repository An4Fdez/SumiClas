namespace SumiClas.Classes.Domain;

public interface IClassRepository
{
    Task<Class?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<Class>> GetBySchoolYearIdAsync(Guid schoolYearId, CancellationToken ct = default);
    Task AddAsync(Class schoolClass, CancellationToken ct = default);
}
