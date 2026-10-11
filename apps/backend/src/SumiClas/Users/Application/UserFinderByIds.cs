using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Users.Domain;

namespace SumiClas.Users.Application;

public static class UserFinderByIds
{
    /// <summary>Returns only the users that exist, so callers compare it with what they asked for.</summary>
    public sealed record Query(IReadOnlyCollection<Guid> UserIds) : IQuery<IReadOnlyCollection<UserResponse>>;

    public sealed class Finder(IUserRepository repository) : IUseCase
    {
        public async Task<IReadOnlyCollection<UserResponse>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var users = await repository.SearchAsync(new UserSpecification(ids: query.UserIds), ct);
            return users.Select(user => new UserResponse(user.Id, user.FullName)).ToList();
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, IReadOnlyCollection<UserResponse>>
    {
        public Task<IReadOnlyCollection<UserResponse>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
