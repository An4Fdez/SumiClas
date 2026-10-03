using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Users.Domain.Services;

namespace SumiClas.Users.Application;

public static class UserIsClassRepresentative
{
    public sealed record Query(Guid UserId, Guid ClassId) : IQuery<bool>;

    public sealed class Handler(UsersByClassRepresentativeFinder finder) : IRequestHandler<Query, bool>
    {
        public async Task<bool> Handle(Query query, CancellationToken ct)
        {
            var matches = await finder.FindAsync(query.UserId, query.ClassId, ct);
            return matches.Count > 0;
        }
    }
}
