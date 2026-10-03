using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Users.Domain;

namespace SumiClas.Users.Application;

public static class UserIsClassRepresentative
{
    public sealed record Query(Guid UserId, Guid ClassId) : IQuery<bool>;

    public sealed class Handler(IUserRepository users) : IRequestHandler<Query, bool>
    {
        public async Task<bool> Handle(Query query, CancellationToken ct)
        {
            var matches = await users.SearchAsync(
                new UserSpecification(id: query.UserId, classId: query.ClassId, classRole: ClassRoleType.ClassRepresentative),
                ct);
            return matches.Count > 0;
        }
    }
}
