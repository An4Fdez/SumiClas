using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Domain;

namespace SumiClas.Users.Application;

public static class UserClassRepresentativeEnsure
{
    /// <summary>Succeeds when the user is a delegate of the class. Other use cases ask this before doing what only a delegate may do.</summary>
    public sealed record Query(Guid UserId, Guid ClassId) : IQuery<Result>;

    public sealed class Ensure(IUserRepository repository) : IUseCase
    {
        public async Task<Result> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var matches = await repository.SearchAsync(
                new UserSpecification(id: query.UserId, classId: query.ClassId, classRole: ClassRoleType.ClassRepresentative),
                ct);

            return matches.Count > 0
                ? Result.Success()
                : Result.Failure(UserErrors.ClassRepresentativeForbidden);
        }
    }

    public sealed class Handler(Ensure ensure) : IRequestHandler<Query, Result>
    {
        public Task<Result> Handle(Query query, CancellationToken ct)
            => ensure.ExecuteAsync(query, ct);
    }
}
