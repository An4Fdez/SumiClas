using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Domain.Services;

namespace SumiClas.Users.Application;

public static class UserProfileFinderCurrent
{
    /// <summary>The profile (with class roles) of the current user.</summary>
    public sealed record Query : IQuery<Result<UserProfileResponse>>;

    public sealed class Finder(UserFinder finder, IUserProvider userProvider) : IUseCase
    {
        public async Task<Result<UserProfileResponse>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var current = userProvider.GetCurrentUserId();

            var found = await finder.FindByIdAsync(current, ct);
            if (found.IsFailure)
            {
                return Result<UserProfileResponse>.Failure(found.Error!);
            }

            var user = found.Value;
            return Result<UserProfileResponse>.Success(new UserProfileResponse(
                user.Id,
                user.FullName,
                user.Email.Value,
                user.ClassRoles.Select(role => new UserClassRoleResponse(role.ClassId, role.Role.ToString())).ToList()));
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, Result<UserProfileResponse>>
    {
        public Task<Result<UserProfileResponse>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
