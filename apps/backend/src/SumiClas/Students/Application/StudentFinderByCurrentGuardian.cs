using MediatR;
using SumiClas.Classes.Application;
using SumiClas.Enrollments.Application;
using SumiClas.Schools.Application;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Students.Domain;

namespace SumiClas.Students.Application;

public static class StudentFinderByCurrentGuardian
{
    /// <summary>The students of the current user (as guardian), with their current class and school.</summary>
    public sealed record Query : IQuery<Result<IReadOnlyCollection<ChildResponse>>>;

    public sealed class Finder(IStudentRepository repository, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
    {
        public async Task<Result<IReadOnlyCollection<ChildResponse>>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var current = userProvider.GetCurrentUserId();
            if (current.IsFailure)
            {
                return Result<IReadOnlyCollection<ChildResponse>>.Failure(current.Error!);
            }

            var students = await repository.SearchAsync(new StudentSpecification(guardianId: current.Value), ct);
            var studentIds = students.Select(student => student.Id).ToList();
            if (studentIds.Count == 0)
            {
                return Result<IReadOnlyCollection<ChildResponse>>.Success(new List<ChildResponse>());
            }

            var enrollments = (await queryBus.Send(new EnrollmentFinderByStudents.Query(studentIds), ct))
                .Where(enrollment => enrollment.Status == "Confirmed")
                .ToList();

            var classes = (await queryBus.Send(
                new ClassFinderByIds.Query(enrollments.Select(enrollment => enrollment.ClassId).Distinct().ToList()), ct))
                .ToDictionary(schoolClass => schoolClass.Id);

            var activeYears = (await queryBus.Send(
                new SchoolYearFinderByIds.Query(classes.Values.Select(schoolClass => schoolClass.SchoolYearId).Distinct().ToList()), ct))
                .Where(schoolYear => schoolYear.Status == "Active")
                .ToDictionary(schoolYear => schoolYear.Id);

            var schools = (await queryBus.Send(
                new SchoolFinderByIds.Query(activeYears.Values.Select(schoolYear => schoolYear.SchoolId).Distinct().ToList()), ct))
                .ToDictionary(school => school.Id);

            IReadOnlyCollection<ChildResponse> response = students
                .OrderBy(student => student.Name.FirstName)
                .Select(student =>
                {
                    var schoolClass = enrollments
                        .Where(enrollment => enrollment.StudentId == student.Id)
                        .Select(enrollment => classes.GetValueOrDefault(enrollment.ClassId))
                        .FirstOrDefault(candidate => candidate is not null && activeYears.ContainsKey(candidate.SchoolYearId));
                    var schoolYear = schoolClass is null ? null : activeYears[schoolClass.SchoolYearId];
                    var school = schoolYear is null ? null : schools.GetValueOrDefault(schoolYear.SchoolId);

                    return new ChildResponse(
                        student.Id,
                        student.Name.FirstName,
                        student.Name.FirstSurname[..1] + ".",
                        schoolClass?.Id,
                        schoolClass?.Name,
                        schoolYear?.Id,
                        school?.Id,
                        school?.Name);
                })
                .ToList();

            return Result<IReadOnlyCollection<ChildResponse>>.Success(response);
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, Result<IReadOnlyCollection<ChildResponse>>>
    {
        public Task<Result<IReadOnlyCollection<ChildResponse>>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
