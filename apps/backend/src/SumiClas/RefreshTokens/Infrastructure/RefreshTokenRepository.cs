using SumiClas.RefreshTokens.Domain;
using SumiClas.Shared.Infrastructure.Persistence;

namespace SumiClas.RefreshTokens.Infrastructure;

public sealed class RefreshTokenRepository(SumiClasDbContext dbContext)
    : BaseRepository<RefreshToken>(dbContext), IRefreshTokenRepository;
