using SumiClas.Shared.Domain;

namespace SumiClas.Localities.Domain;

public interface ILocalityRepository
{
    Task<Locality?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<Locality>> SearchAsync(ISpecification<Locality> specification, CancellationToken ct = default);
    Task AddAsync(Locality locality, CancellationToken ct = default);
}
