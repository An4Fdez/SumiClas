using MediatR;
using SumiClas.Enrollments.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Students.Application;
using SumiClas.Users.Domain;

namespace SumiClas.Users.Application;

public static class UserClassAccessEnsure
{
    private const string ConfirmedStatus = "Confirmed";

    /// <summary>
    /// Succeeds when the user belongs to the class: a delegate of the class, or a guardian of a student with a
    /// confirmed enrollment in it. Other use cases ask this before reading or changing the data of a class.
    /// </summary>
    public sealed record Query(Guid UserId, Guid ClassId) : IQuery<Result>;

    public sealed class Ensure(IUserRepository repository, IQueryBus queryBus) : IUseCase
    {
        public async Task<Result> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var representatives = await repository.SearchAsync(
                new UserSpecification(id: query.UserId, classId: query.ClassId, classRole: ClassRoleType.ClassRepresentative),
                ct);
            if (representatives.Count > 0)
            {
                return Result.Success();
            }

            var enrollments = await queryBus.Send(new EnrollmentFinderByClass.Query(query.ClassId), ct);
            var studentIds = enrollments
                .Where(enrollment => enrollment.Status == ConfirmedStatus)
                .Select(enrollment => enrollment.StudentId)
                .Distinct()
                .ToList();
            var guardianIds = await queryBus.Send(new StudentGuardianFinderByStudents.Query(studentIds), ct);

            return guardianIds.Contains(query.UserId)
                ? Result.Success()
                : Result.Failure(UserErrors.ClassAccessForbidden);
        }
    }

    public sealed class Handler(Ensure ensure) : IRequestHandler<Query, Result>
    {
        public Task<Result> Handle(Query query, CancellationToken ct)
            => ensure.ExecuteAsync(query, ct);
    }
}
