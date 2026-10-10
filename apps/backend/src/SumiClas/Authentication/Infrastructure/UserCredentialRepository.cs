using SumiClas.Authentication.Domain;
using SumiClas.Shared.Infrastructure.Persistence;

namespace SumiClas.Authentication.Infrastructure;

public sealed class UserCredentialRepository(SumiClasDbContext dbContext)
    : BaseRepository<UserCredential>(dbContext), IUserCredentialRepository;
