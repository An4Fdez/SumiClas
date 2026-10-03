using MediatR;
using SumiClas.Users.Domain;
using SumiClas.Shared.Application;

namespace SumiClas.Users.Application;

public static class UserClassMemberFinderByIds
{
    /// <summary>Returns only the users that exist, with the role each one holds in <paramref name="ClassId"/>.</summary>
    public sealed record Query(Guid ClassId, IReadOnlyCollection<Guid> UserIds)
        : IQuery<IReadOnlyCollection<UserClassMemberResponse>>;

    public sealed class Handler(IUserRepository userRepository)
        : IRequestHandler<Query, IReadOnlyCollection<UserClassMemberResponse>>
    {
        public async Task<IReadOnlyCollection<UserClassMemberResponse>> Handle(Query query, CancellationToken ct)
        {
            var users = await userRepository.SearchAsync(new UserSpecification(ids: query.UserIds), ct);
            return users
                .Select(user => new UserClassMemberResponse(
                    user.Id,
                    user.FullName,
                    user.ClassRoles.FirstOrDefault(role => role.ClassId == query.ClassId)?.Role.ToString()))
                .ToList();
        }
    }
}
