using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Users.Domain.Services;

namespace SumiClas.Users.Application;

public static class UserProfileFinderCurrent
{
    /// <summary>The profile (with class roles) of the current user.</summary>
    public sealed record Query : IQuery<UserProfileResponse>;

    public sealed class Finder(UserFinder finder, IUserProvider userProvider) : IUseCase
    {
        public async Task<UserProfileResponse> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var current = userProvider.GetCurrentUserId();

            var user = await finder.FindByIdAsync(current, ct);
            return new UserProfileResponse(
                user.Id,
                user.FullName,
                user.Email.Value,
                user.ClassRoles.Select(role => new UserClassRoleResponse(role.ClassId, role.Role.ToString())).ToList());
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, UserProfileResponse>
    {
        public Task<UserProfileResponse> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
