using SumiClas.Shared.Domain;

namespace SumiClas.PostalCodes.Domain;

public interface IPostalCodeRepository
{
    Task<PostalCode?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<PostalCode>> SearchAsync(ISpecification<PostalCode> specification, CancellationToken ct = default);
    Task AddAsync(PostalCode postalCode, CancellationToken ct = default);
}
