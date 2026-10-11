using SumiClas.Authentication.Domain;
using SumiClas.Shared.Infrastructure.Persistence;

namespace SumiClas.Authentication.Infrastructure;

public sealed class RefreshTokenRepository(SumiClasDbContext dbContext)
    : BaseRepository<RefreshToken>(dbContext), IRefreshTokenRepository;
