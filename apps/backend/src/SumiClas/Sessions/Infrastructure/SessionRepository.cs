using SumiClas.Sessions.Domain;
using SumiClas.Shared.Infrastructure.Persistence;

namespace SumiClas.Sessions.Infrastructure;

public sealed class SessionRepository(SumiClasDbContext dbContext)
    : BaseRepository<Session>(dbContext), ISessionRepository;
