using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Users.Domain;

namespace SumiClas.Users.Application;

public static class UserAnyClassRepresentativeEnsure
{
    /// <summary>Completes when the user is a delegate of at least one of the classes; with no classes it never completes.</summary>
    public sealed record Query(Guid UserId, IReadOnlyCollection<Guid> ClassIds) : IQuery<Unit>;

    public sealed class Ensure(IUserRepository repository) : IUseCase
    {
        public async Task<Unit> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            IReadOnlyCollection<User> matches = await repository.SearchAsync(
                new UserSpecification(id: query.UserId, classIds: query.ClassIds, classRole: ClassRoleType.ClassRepresentative),
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
