namespace SumiClas.Users.Domain.Services;

public sealed class UserFinder(IUserRepository users)
{
    public async Task<User> FindAsync(Guid id, CancellationToken ct = default)
        => await users.GetByIdAsync(id, ct) ?? throw new UserExceptions.NotFound();
}
