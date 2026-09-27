namespace SumiClas.Schools.Domain;

public interface ISchoolRepository
{
    Task<School?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(School school, CancellationToken ct = default);
}
