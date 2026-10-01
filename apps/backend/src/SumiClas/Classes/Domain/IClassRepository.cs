using SumiClas.Shared.Domain;

namespace SumiClas.Classes.Domain;

public interface IClassRepository
{
    Task<Class?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<Class>> SearchAsync(ISpecification<Class> specification, CancellationToken ct = default);
    Task AddAsync(Class schoolClass, CancellationToken ct = default);
}
