using SumiClas.Shared.Domain;

namespace SumiClas.Users.Domain.Services;

public sealed class UserFinder(IUserRepository users)
{
    public async Task<Result<User>> FindByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(id, ct);
        return user is null
            ? Result<User>.Failure(UserErrors.NotFound)
            : Result<User>.Success(user);
    }
}
