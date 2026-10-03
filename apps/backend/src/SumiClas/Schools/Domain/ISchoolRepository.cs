using SumiClas.Shared.Domain;

namespace SumiClas.Schools.Domain;

public interface ISchoolRepository
{
    Task<School?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<School>> SearchAsync(ISpecification<School> specification, CancellationToken ct = default);
    Task AddAsync(School school, CancellationToken ct = default);
}
