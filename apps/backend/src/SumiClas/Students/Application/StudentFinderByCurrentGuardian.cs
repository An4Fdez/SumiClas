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
    public sealed record Query : IQuery<IReadOnlyCollection<ChildResponse>>;

    public sealed class Finder(IStudentRepository repository, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
    {
        public async Task<IReadOnlyCollection<ChildResponse>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            Guid current = userProvider.GetCurrentUserId();

            IReadOnlyCollection<Student> students = await repository.SearchAsync(new StudentSpecification(guardianId: current), ct);
            List<Guid> studentIds = students.Select(student => student.Id).ToList();
            if (studentIds.Count == 0)
            {
                return new List<ChildResponse>();
            }

            List<EnrollmentResponse> enrollments = (await queryBus.Send(new EnrollmentFinderByStudents.Query(studentIds), ct))
                .Where(enrollment => enrollment.Status == "Confirmed")
                .ToList();

            Dictionary<Guid, ClassResponse> classes = (await queryBus.Send(
                new ClassFinderByIds.Query(enrollments.Select(enrollment => enrollment.ClassId).Distinct().ToList()), ct))
                .ToDictionary(schoolClass => schoolClass.Id);

            Dictionary<Guid, SchoolYearResponse> activeYears = (await queryBus.Send(
                new SchoolYearFinderByIds.Query(classes.Values.Select(schoolClass => schoolClass.SchoolYearId).Distinct().ToList()), ct))
                .Where(schoolYear => schoolYear.Status == "Active")
                .ToDictionary(schoolYear => schoolYear.Id);

            Dictionary<Guid, SchoolResponse> schools = (await queryBus.Send(
                new SchoolFinderByIds.Query(activeYears.Values.Select(schoolYear => schoolYear.SchoolId).Distinct().ToList()), ct))
                .ToDictionary(school => school.Id);

            return students
                .OrderBy(student => student.Name.FirstName)
                .Select(student =>
                {
                    ClassResponse? schoolClass = enrollments
                        .Where(enrollment => enrollment.StudentId == student.Id)
                        .Select(enrollment => classes.GetValueOrDefault(enrollment.ClassId))
                        .FirstOrDefault(candidate => candidate is not null && activeYears.ContainsKey(candidate.SchoolYearId));
                    SchoolYearResponse? schoolYear = schoolClass is null ? null : activeYears[schoolClass.SchoolYearId];
                    SchoolResponse? school = schoolYear is null ? null : schools.GetValueOrDefault(schoolYear.SchoolId);

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
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, IReadOnlyCollection<ChildResponse>>
    {
        public Task<IReadOnlyCollection<ChildResponse>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
