using SumiClas.Shared.Domain;

namespace SumiClas.Users.Domain;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<User>> SearchAsync(ISpecification<User> specification, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
}
