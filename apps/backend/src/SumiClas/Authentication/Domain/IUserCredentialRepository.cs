namespace SumiClas.Authentication.Domain;

public interface IUserCredentialRepository
{
    Task<UserCredential?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(UserCredential credential, CancellationToken ct = default);
}
