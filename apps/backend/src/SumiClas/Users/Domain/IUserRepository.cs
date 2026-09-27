namespace SumiClas.Users.Domain;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(Email email, CancellationToken ct = default);
    Task<int> CountClassRepresentativesAsync(Guid classId, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
}
