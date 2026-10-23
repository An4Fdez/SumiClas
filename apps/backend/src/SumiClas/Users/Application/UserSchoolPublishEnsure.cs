using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Users.Domain;

namespace SumiClas.Users.Application;

public static class UserSchoolPublishEnsure
{
    /// <summary>
    /// Completes when the user may publish to the whole school. Today nobody may: the admin and teacher roles that
    /// the architecture document reserves that scope for do not exist yet, so this is where their check will go.
    /// </summary>
    public sealed record Query(Guid UserId, Guid SchoolId) : IQuery<Unit>;

    public sealed class Ensure : IUseCase
    {
        public Task<Unit> ExecuteAsync(Query query, CancellationToken ct = default)
            => Task.FromException<Unit>(new UserExceptions.SchoolPublishForbidden());
    }

    public sealed class Handler(Ensure ensure) : IRequestHandler<Query, Unit>
    {
        public Task<Unit> Handle(Query query, CancellationToken ct)
            => ensure.ExecuteAsync(query, ct);
    }
}
