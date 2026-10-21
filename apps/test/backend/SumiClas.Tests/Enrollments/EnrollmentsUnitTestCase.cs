namespace SumiClas.Tests.Enrollments;

using MediatR;
using Moq;
using SumiClas.Classes.Application;
using SumiClas.Classes.Domain;
using SumiClas.Enrollments.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Students.Application;
using SumiClas.Students.Domain;
using SumiClas.Tests.Classes.Application;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Students.Application;

public abstract class EnrollmentsUnitTestCase : UnitTestCase
{
    private readonly List<Enrollment> _added = [];

    protected Mock<IEnrollmentRepository> Repository { get; } = new();

    protected IReadOnlyList<Enrollment> Added => this._added;

    [SetUp]
    public void EnrollmentsSetUp()
    {
        this._added.Clear();
        this.Repository.Reset();
    }

    protected void ShouldFetchEnrollments(params Enrollment[] enrollments)
        => this.Repository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _)
                => enrollments.FirstOrDefault(enrollment => enrollment.Id == id));

    protected void ShouldSearchEnrollments(params Enrollment[] enrollments)
        => this.Repository
            .Setup(repository => repository.SearchAsync(
                It.IsAny<ISpecification<Enrollment>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<Enrollment> specification, CancellationToken _)
                => (IReadOnlyCollection<Enrollment>)enrollments.Concat(this._added)
                    .Where(specification.Criteria.Compile()).ToList());

    protected void ShouldSearchNoEnrollments()
        => this.ShouldSearchEnrollments();

    protected void ShouldCaptureAddedEnrollments()
        => this.Repository
            .Setup(repository => repository.AddAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
            .Callback<Enrollment, CancellationToken>((enrollment, _) => this._added.Add(enrollment))
            .Returns(Task.CompletedTask);

    protected void ShouldFindClassInSchoolYear(Guid classId, Guid schoolYearId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<ClassFinderById.Query>(query => query.ClassId == classId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ClassResponseMother.With(classId, schoolYearId));

    protected void ShouldFindClassesOfSchoolYear(Guid schoolYearId, params Guid[] classIds)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<ClassFinderBySchoolYear.Query>(query => query.SchoolYearId == schoolYearId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(classIds.Select(classId => ClassResponseMother.With(classId, schoolYearId)).ToList());

    protected void ShouldNotFindClass()
        => this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<IQuery<ClassResponse>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ClassExceptions.NotFound());

    protected void ShouldEnsureSchoolYearIsActive(Guid schoolYearId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<SchoolYearEnsureIsActive.Query>(query => query.SchoolYearId == schoolYearId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

    protected void ShouldFailEnsuringSchoolYearIsActive(Guid schoolYearId, Exception exception)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<SchoolYearEnsureIsActive.Query>(query => query.SchoolYearId == schoolYearId),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

    protected void ShouldEnsureStudentGuardian(Guid studentId, Guid userId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<StudentGuardianEnsure.Query>(query => query.StudentId == studentId && query.UserId == userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

    protected void ShouldFailEnsuringStudentGuardian(Guid studentId, Guid userId, Error error)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<StudentGuardianEnsure.Query>(query => query.StudentId == studentId && query.UserId == userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(error));

}
