using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Users.Domain;

namespace SumiClas.Users.Application;

public static class UserFinderByIdOrNull
{
    /// <summary>The user, or <c>null</c> when nobody has that id: for the callers that must not fail on a missing user.</summary>
    public sealed record Query(Guid UserId) : IQuery<UserResponse?>;

    public sealed class Finder(IUserRepository repository) : IUseCase
    {
        public async Task<UserResponse?> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            User? user = await repository.GetByIdAsync(query.UserId, ct);
            return user is null ? null : new UserResponse(user.Id, user.FullName);
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, UserResponse?>
    {
        public Task<UserResponse?> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
