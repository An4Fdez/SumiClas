namespace SumiClas.Tests.Classes;

using MediatR;
using Moq;
using SumiClas.Classes.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

public abstract class ClassesUnitTestCase : UnitTestCase
{
    private readonly List<Class> _added = [];

    protected Mock<IClassRepository> Repository { get; } = new();

    protected IReadOnlyList<Class> Added => this._added;

    [SetUp]
    public void ClassesSetUp()
    {
        this._added.Clear();
        this.Repository.Reset();
    }

    protected void ShouldFetchClasses(params Class[] classes)
        => this.Repository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _)
                => classes.FirstOrDefault(schoolClass => schoolClass.Id == id));

    protected void ShouldSearchClasses(params Class[] classes)
        => this.Repository
            .Setup(repository => repository.SearchAsync(
                It.IsAny<ISpecification<Class>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<Class> specification, CancellationToken _)
                => InMemorySearch.Run(classes, specification));

    protected void ShouldFetchNoClasses()
        => this.ShouldFetchClasses();

    protected void ShouldCaptureAddedClasses()
        => this.Repository
            .Setup(repository => repository.AddAsync(It.IsAny<Class>(), It.IsAny<CancellationToken>()))
            .Callback<Class, CancellationToken>((schoolClass, _) => this._added.Add(schoolClass))
            .Returns(Task.CompletedTask);

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
