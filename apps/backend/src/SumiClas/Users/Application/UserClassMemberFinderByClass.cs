using MediatR;
using SumiClas.Classes.Application;
using SumiClas.Enrollments.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Students.Application;
using SumiClas.Users.Domain;

namespace SumiClas.Users.Application;

public static class UserClassMemberFinderByClass
{
    private const string ConfirmedStatus = "Confirmed";

    /// <summary>
    /// The members of a class: the guardians of the students with a confirmed enrollment plus the class delegates.
    /// Participants have no stored role, so they are computed. Only members of the class may ask.
    /// </summary>
    public sealed record Query(Guid ClassId) : IQuery<Result<IReadOnlyCollection<UserClassMemberResponse>>>;

    public sealed class Finder(IUserRepository userRepository, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
    {
        public async Task<Result<IReadOnlyCollection<UserClassMemberResponse>>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var current = userProvider.GetCurrentUserId();
            if (current.IsFailure)
            {
                return Result<IReadOnlyCollection<UserClassMemberResponse>>.Failure(current.Error!);
            }

            var schoolClass = await queryBus.Send(new ClassFinderById.Query(query.ClassId), ct);
            if (schoolClass.IsFailure)
            {
                return Result<IReadOnlyCollection<UserClassMemberResponse>>.Failure(schoolClass.Error!);
            }

            var enrollments = await queryBus.Send(new EnrollmentFinderByClass.Query(query.ClassId), ct);
            var studentIds = enrollments
                .Where(enrollment => enrollment.Status == ConfirmedStatus)
                .Select(enrollment => enrollment.StudentId)
                .Distinct()
                .ToList();
            var guardianIds = await queryBus.Send(new StudentGuardianFinderByStudents.Query(studentIds), ct);

            var delegates = await userRepository.SearchAsync(
                new UserSpecification(classId: query.ClassId, classRole: ClassRoleType.ClassRepresentative), ct);
            var memberIds = guardianIds.Concat(delegates.Select(user => user.Id)).Distinct().ToList();

            var users = await userRepository.SearchAsync(new UserSpecification(ids: memberIds), ct);
            if (users.All(user => user.Id != current.Value))
            {
                return Result<IReadOnlyCollection<UserClassMemberResponse>>.Failure(UserErrors.ClassMembersForbidden);
            }

            var representativeRole = ClassRoleType.ClassRepresentative.ToString();
            IReadOnlyCollection<UserClassMemberResponse> members = users
                .Select(user => new UserClassMemberResponse(
                    user.Id,
                    user.FullName,
                    user.ClassRoles.FirstOrDefault(role => role.ClassId == query.ClassId)?.Role.ToString()
                        ?? ClassRoleType.Participant.ToString()))
                .OrderBy(member => member.ClassRole == representativeRole ? 0 : 1)
                .ThenBy(member => member.FullName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return Result<IReadOnlyCollection<UserClassMemberResponse>>.Success(members);
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, Result<IReadOnlyCollection<UserClassMemberResponse>>>
    {
        public Task<Result<IReadOnlyCollection<UserClassMemberResponse>>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
