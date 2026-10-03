using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Domain.Services;

namespace SumiClas.Users.Application;

public static class UserProfileFinderCurrent
{
    /// <summary>The profile (with class roles) of the current user.</summary>
    public sealed record Query : IQuery<Result<UserProfileResponse>>;

    public sealed class Handler(UserFinder finder, IUserProvider userProvider)
        : IRequestHandler<Query, Result<UserProfileResponse>>
    {
        public async Task<Result<UserProfileResponse>> Handle(Query query, CancellationToken ct)
        {
            var current = userProvider.GetCurrentUserId();
            if (current.IsFailure)
            {
                return Result<UserProfileResponse>.Failure(current.Error!);
            }

            var found = await finder.FindByIdAsync(current.Value, ct);
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
}
