using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Users.Domain;

namespace SumiClas.Users.Application;

public static class UserClassRepresentativeEnsure
{
    /// <summary>Completes when the user is a delegate of the class. Other use cases ask this before doing what only a delegate may do.</summary>
    public sealed record Query(Guid UserId, Guid ClassId) : IQuery<Unit>;

    public sealed class Ensure(IUserRepository repository) : IUseCase
    {
        public async Task<Unit> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            IReadOnlyCollection<User> matches = await repository.SearchAsync(
                new UserSpecification(id: query.UserId, classId: query.ClassId, classRole: ClassRoleType.ClassRepresentative),
                ct);

            return matches.Count > 0
                ? Unit.Value
                : throw new UserExceptions.ClassRepresentativeForbidden();
        }
    }

    public sealed class Handler(Ensure ensure) : IRequestHandler<Query, Unit>
    {
        public Task<Unit> Handle(Query query, CancellationToken ct)
            => ensure.ExecuteAsync(query, ct);
    }
}
