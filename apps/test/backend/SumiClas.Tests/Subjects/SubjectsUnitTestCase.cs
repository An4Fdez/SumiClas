namespace SumiClas.Tests.Subjects;

using MediatR;
using Moq;
using SumiClas.Classes.Application;
using SumiClas.Classes.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Subjects.Domain;
using SumiClas.Tests.Classes.Application;
using SumiClas.Tests.Shared;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

public abstract class SubjectsUnitTestCase : UnitTestCase
{
    private readonly List<Subject> _added = [];

    protected Mock<ISubjectRepository> Repository { get; } = new();

    protected IReadOnlyList<Subject> Added => this._added;

    [SetUp]
    public void SubjectsSetUp()
    {
        this._added.Clear();
        this.Repository.Reset();
    }

    protected void ShouldFetchSubjects(params Subject[] subjects)
        => this.Repository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _)
                => subjects.FirstOrDefault(subject => subject.Id == id));

    protected void ShouldSearchSubjects(params Subject[] subjects)
        => this.Repository
            .Setup(repository => repository.SearchAsync(
                It.IsAny<ISpecification<Subject>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<Subject> specification, CancellationToken _)
                => InMemorySearch.Run(subjects, specification));

    protected void ShouldCaptureAddedSubjects()
        => this.Repository
            .Setup(repository => repository.AddAsync(It.IsAny<Subject>(), It.IsAny<CancellationToken>()))
            .Callback<Subject, CancellationToken>((subject, _) => this._added.Add(subject))
            .Returns(Task.CompletedTask);

    protected void ShouldNotHaveSearched()
        => this.Repository.Verify(
            repository => repository.SearchAsync(
                It.IsAny<ISpecification<Subject>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

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

    protected void ShouldFindClassInSchoolYear(Guid classId, Guid schoolYearId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<ClassFinderById.Query>(query => query.ClassId == classId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ClassResponseMother.With(classId, schoolYearId));

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

    protected void ShouldAllowClassRepresentative(Guid userId, Guid classId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<UserClassRepresentativeEnsure.Query>(query => query.UserId == userId && query.ClassId == classId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

    protected void ShouldDenyClassRepresentative(Guid userId, Guid classId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<UserClassRepresentativeEnsure.Query>(query => query.UserId == userId && query.ClassId == classId),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UserExceptions.ClassRepresentativeForbidden());
}
