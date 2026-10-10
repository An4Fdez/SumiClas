using SumiClas.Shared.Domain;

namespace SumiClas.Provinces.Domain;

public interface IProvinceRepository
{
    Task<Province?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<Province>> SearchAsync(ISpecification<Province> specification, CancellationToken ct = default);
    Task AddAsync(Province province, CancellationToken ct = default);
}
