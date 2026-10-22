using MediatR;
using SumiClas.Enrollments.Application;
using SumiClas.Shared.Application;
using SumiClas.Students.Application;
using SumiClas.Users.Domain;

namespace SumiClas.Users.Application;

public static class UserIsClassMember
{
    private const string ConfirmedStatus = "Confirmed";

    /// <summary>
    /// Whether the user belongs to the class: a delegate of the class, or a guardian of a student with a confirmed
    /// enrollment in it. For the callers that must branch on it instead of failing (see <see cref="UserClassAccessEnsure"/>).
    /// </summary>
    public sealed record Query(Guid UserId, Guid ClassId) : IQuery<bool>;

    public sealed class Check(IUserRepository repository, IQueryBus queryBus) : IUseCase
    {
        public async Task<bool> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            IReadOnlyCollection<User> representatives = await repository.SearchAsync(
                new UserSpecification(id: query.UserId, classId: query.ClassId, classRole: ClassRoleType.ClassRepresentative),
                ct);
            if (representatives.Count > 0)
            {
                return true;
            }

            IReadOnlyCollection<EnrollmentResponse> enrollments = await queryBus.Send(new EnrollmentFinderByClass.Query(query.ClassId), ct);
            List<Guid> studentIds = enrollments
                .Where(enrollment => enrollment.Status == ConfirmedStatus)
                .Select(enrollment => enrollment.StudentId)
                .Distinct()
                .ToList();
            IReadOnlyCollection<Guid> guardianIds = await queryBus.Send(new StudentGuardianFinderByStudents.Query(studentIds), ct);

            return guardianIds.Contains(query.UserId);
        }
    }

    public sealed class Handler(Check check) : IRequestHandler<Query, bool>
    {
        public Task<bool> Handle(Query query, CancellationToken ct)
            => check.ExecuteAsync(query, ct);
    }
}
