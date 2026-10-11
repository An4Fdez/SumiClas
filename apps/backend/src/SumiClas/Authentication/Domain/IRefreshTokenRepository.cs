using SumiClas.Shared.Domain;

namespace SumiClas.Authentication.Domain;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<RefreshToken>> SearchAsync(ISpecification<RefreshToken> specification, CancellationToken ct = default);
    Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default);
}
