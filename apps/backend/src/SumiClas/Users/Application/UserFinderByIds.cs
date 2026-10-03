using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Users.Domain.Services;

namespace SumiClas.Users.Application;

public static class UserFinderByIds
{
    /// <summary>Returns only the users that exist, so callers compare it with what they asked for.</summary>
    public sealed record Query(IReadOnlyCollection<Guid> UserIds) : IQuery<IReadOnlyCollection<UserResponse>>;

    public sealed class Handler(UsersByIdsFinder finder) : IRequestHandler<Query, IReadOnlyCollection<UserResponse>>
    {
        public async Task<IReadOnlyCollection<UserResponse>> Handle(Query query, CancellationToken ct)
        {
            var users = await finder.FindAsync(query.UserIds, ct);
            return users.Select(user => new UserResponse(user.Id, user.FullName)).ToList();
        }
    }
}
