namespace SumiClas.Tests.Assignments;

using MediatR;
using Moq;
using SumiClas.Assignments.Domain;
using SumiClas.Classes.Application;
using SumiClas.Classes.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Subjects.Application;
using SumiClas.Subjects.Domain;
using SumiClas.Tests.Classes.Application;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Subjects.Application;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

public abstract class AssignmentsUnitTestCase : UnitTestCase
{
    private readonly List<Assignment> _added = [];

    protected Mock<IAssignmentRepository> Repository { get; } = new();

    protected IReadOnlyList<Assignment> Added => this._added;

    [SetUp]
    public void AssignmentsSetUp()
    {
        this._added.Clear();
        this.Repository.Reset();
    }

    protected void ShouldCaptureAddedAssignments()
        => this.Repository
            .Setup(repository => repository.AddAsync(It.IsAny<Assignment>(), It.IsAny<CancellationToken>()))
            .Callback<Assignment, CancellationToken>((assignment, _) => this._added.Add(assignment))
            .Returns(Task.CompletedTask);

    protected void ShouldFetchAssignments(params Assignment[] assignments)
        => this.Repository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _)
                => assignments.FirstOrDefault(assignment => assignment.Id == id));

    protected void ShouldSearchAssignments(params Assignment[] assignments)
        => this.Repository
            .Setup(repository => repository.SearchAsync(
                It.IsAny<ISpecification<Assignment>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<Assignment> specification, CancellationToken _)
                => InMemorySearch.Run(assignments, specification));

    protected void ShouldFetchNoAssignments()
        => this.ShouldFetchAssignments();

    protected void ShouldSearchNoAssignments()
        => this.ShouldSearchAssignments();

    protected void ShouldNotHaveSearched()
        => this.Repository.Verify(
            repository => repository.SearchAsync(
                It.IsAny<ISpecification<Assignment>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldNotHaveFetched()
        => this.Repository.Verify(
            repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldFindUser(Guid userId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<UserFinderById.Query>(query => query.UserId == userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(UserResponseMother.With(userId, StringMother.Random("name")));

    protected void ShouldNotFindUser(Guid userId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<UserFinderById.Query>(query => query.UserId == userId),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UserExceptions.NotFound());

    protected void ShouldFindClassInSchoolYear(Guid classId, Guid schoolYearId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<ClassFinderById.Query>(query => query.ClassId == classId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ClassResponseMother.With(classId, schoolYearId));

    protected void ShouldFindClass(Guid classId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<ClassFinderById.Query>(query => query.ClassId == classId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ClassResponseMother.WithId(classId));

    protected void ShouldNotFindClass()
        => this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<IQuery<ClassResponse>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ClassExceptions.NotFound());

    protected void ShouldHaveAskedForClass(Guid classId)
        => this.QueryBus.Verify(
            bus => bus.Send(
                It.Is<ClassFinderById.Query>(query => query.ClassId == classId),
                It.IsAny<CancellationToken>()),
            Times.Once);

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

    protected void ShouldFindSubject(Guid subjectId, Guid classId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<SubjectFinderById.Query>(query => query.SubjectId == subjectId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(SubjectResponseMother.With(subjectId, classId, StringMother.Random("subject")));

    protected void ShouldNotFindSubject(Guid subjectId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<SubjectFinderById.Query>(query => query.SubjectId == subjectId),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new SubjectExceptions.NotFound());

    protected void ShouldHaveClassRepresentative(Guid userId, Guid classId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<UserIsClassRepresentative.Query>(query => query.UserId == userId && query.ClassId == classId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

    protected void ShouldNotHaveClassRepresentative(Guid userId, Guid classId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<UserIsClassRepresentative.Query>(query => query.UserId == userId && query.ClassId == classId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

    protected void ShouldAllowClassAccess(Guid userId, Guid classId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<UserClassAccessEnsure.Query>(query => query.UserId == userId && query.ClassId == classId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

    protected void ShouldDenyClassAccess(Guid userId, Guid classId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<UserClassAccessEnsure.Query>(query => query.UserId == userId && query.ClassId == classId),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UserExceptions.ClassAccessForbidden());
}
