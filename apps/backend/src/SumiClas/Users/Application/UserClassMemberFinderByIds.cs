using MediatR;
using SumiClas.Users.Domain;
using SumiClas.Shared.Application;

namespace SumiClas.Users.Application;

public static class UserClassMemberFinderByIds
{
    /// <summary>Returns only the users that exist, with the role each one holds in <paramref name="ClassId"/>.</summary>
    public sealed record Query(Guid ClassId, IReadOnlyCollection<Guid> UserIds)
        : IQuery<IReadOnlyCollection<UserClassMemberResponse>>;

    public sealed class Finder(IUserRepository userRepository) : IUseCase
    {
        public async Task<IReadOnlyCollection<UserClassMemberResponse>> ExecuteAsync(Query query, CancellationToken ct = default)
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

    public sealed class Handler(Finder finder) : IRequestHandler<Query, IReadOnlyCollection<UserClassMemberResponse>>
    {
        public Task<IReadOnlyCollection<UserClassMemberResponse>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
