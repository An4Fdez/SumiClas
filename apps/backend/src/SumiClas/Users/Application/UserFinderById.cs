using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Users.Domain.Services;

namespace SumiClas.Users.Application;

public static class UserFinderById
{
    public sealed record Query(Guid UserId) : IQuery<UserResponse>;

    public sealed class Finder(UserFinder finder) : IUseCase
    {
        public async Task<UserResponse> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var user = await finder.FindByIdAsync(query.UserId, ct);
            return new UserResponse(user.Id, user.FullName);
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, UserResponse>
    {
        public Task<UserResponse> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
