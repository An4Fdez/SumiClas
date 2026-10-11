using SumiClas.Shared.Domain;

namespace SumiClas.Sessions.Domain;

public interface ISessionRepository
{
    Task<Session?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<Session>> SearchAsync(ISpecification<Session> specification, CancellationToken ct = default);
    Task AddAsync(Session session, CancellationToken ct = default);
}
