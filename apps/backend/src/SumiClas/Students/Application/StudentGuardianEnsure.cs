using MediatR;
using SumiClas.Shared.Application;
using SumiClas.Students.Domain;
using SumiClas.Students.Domain.Services;

namespace SumiClas.Students.Application;

public static class StudentGuardianEnsure
{
    /// <summary>Completes when the user is a guardian of the student. Other use cases ask this before acting on a student.</summary>
    public sealed record Query(Guid StudentId, Guid UserId) : IQuery<Unit>;

    public sealed class Ensure(StudentFinder finder) : IUseCase
    {
        public async Task<Unit> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            Student student = await finder.FindAsync(query.StudentId, ct);

            if (student.Guardians.All(guardian => guardian.GuardianId != query.UserId))
            {
                throw new StudentExceptions.GuardianAccessForbidden();
            }

            return Unit.Value;
        }
    }

    public sealed class Handler(Ensure ensure) : IRequestHandler<Query, Unit>
    {
        public Task<Unit> Handle(Query query, CancellationToken ct)
            => ensure.ExecuteAsync(query, ct);
    }
}
