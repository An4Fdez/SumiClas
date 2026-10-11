using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Domain.Services;

namespace SumiClas.Users.Application;

public static class UserFinderById
{
    public sealed record Query(Guid UserId) : IQuery<Result<UserResponse>>;

    public sealed class Finder(UserFinder finder) : IUseCase
    {
        public async Task<Result<UserResponse>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var found = await finder.FindByIdAsync(query.UserId, ct);
            if (found.IsFailure)
            {
                return Result<UserResponse>.Failure(found.Error!);
            }

            var user = found.Value;
            return Result<UserResponse>.Success(new UserResponse(user.Id, user.FullName));
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, Result<UserResponse>>
    {
        public Task<Result<UserResponse>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
