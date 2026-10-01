using SumiClas.Shared.Infrastructure.Persistence;
using SumiClas.Users.Domain;

namespace SumiClas.Users.Infrastructure;

public sealed class UserRepository(SumiClasDbContext dbContext)
    : BaseRepository<User>(dbContext), IUserRepository;
