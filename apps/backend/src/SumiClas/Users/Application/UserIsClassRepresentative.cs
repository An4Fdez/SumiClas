using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Users.Domain;

namespace SumiClas.Users.Application;

public static class UserIsClassRepresentative
{
    public sealed record Query(Guid UserId, Guid ClassId) : IQuery<bool>;

    public sealed class Check(IUserRepository users) : IUseCase
    {
        public async Task<bool> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var matches = await users.SearchAsync(
                new UserSpecification(id: query.UserId, classId: query.ClassId, classRole: ClassRoleType.ClassRepresentative),
                ct);
            return matches.Count > 0;
        }
    }

    public sealed class Handler(Check check) : IRequestHandler<Query, bool>
    {
        public Task<bool> Handle(Query query, CancellationToken ct)
            => check.ExecuteAsync(query, ct);
    }
}
