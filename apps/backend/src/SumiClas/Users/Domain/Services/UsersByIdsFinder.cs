namespace SumiClas.Users.Domain.Services;

public sealed class UsersByIdsFinder(IUserRepository users)
{
    public Task<IReadOnlyCollection<User>> FindAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default)
        => users.SearchAsync(new UsersByIdsSpecification(ids), ct);
}
