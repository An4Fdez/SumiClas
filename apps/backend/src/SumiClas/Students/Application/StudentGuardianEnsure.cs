using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Students.Domain;
using SumiClas.Students.Domain.Services;

namespace SumiClas.Students.Application;

public static class StudentGuardianEnsure
{
    /// <summary>Succeeds when the user is a guardian of the student. Other use cases ask this before acting on a student.</summary>
    public sealed record Query(Guid StudentId, Guid UserId) : IQuery<Result>;

    public sealed class Ensure(StudentFinder finder) : IUseCase
    {
        public async Task<Result> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var found = await finder.FindByIdAsync(query.StudentId, ct);
            if (found.IsFailure)
            {
                return Result.Failure(found.Error!);
            }

            return found.Value.Guardians.Any(guardian => guardian.GuardianId == query.UserId)
                ? Result.Success()
                : Result.Failure(StudentErrors.GuardianAccessForbidden);
        }
    }

    public sealed class Handler(Ensure ensure) : IRequestHandler<Query, Result>
    {
        public Task<Result> Handle(Query query, CancellationToken ct)
            => ensure.ExecuteAsync(query, ct);
    }
}
