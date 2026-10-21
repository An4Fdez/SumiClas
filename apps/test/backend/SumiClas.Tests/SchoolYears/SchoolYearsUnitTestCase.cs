namespace SumiClas.Tests.SchoolYears;

using MediatR;
using Moq;
using SumiClas.Classes.Application;
using SumiClas.SchoolYears.Domain;
using SumiClas.Schools.Application;
using SumiClas.Schools.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Classes.Application;
using SumiClas.Tests.Shared;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

public abstract class SchoolYearsUnitTestCase : UnitTestCase
{
    private readonly List<SchoolYear> _added = [];

    protected Mock<ISchoolYearRepository> Repository { get; } = new();

    protected IReadOnlyList<SchoolYear> Added => this._added;

    [SetUp]
    public void SchoolYearsSetUp()
    {
        this._added.Clear();
        this.Repository.Reset();
    }

    protected void ShouldFetchSchoolYears(params SchoolYear[] schoolYears)
        => this.Repository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _)
                => schoolYears.FirstOrDefault(schoolYear => schoolYear.Id == id));

    protected void ShouldSearchSchoolYears(params SchoolYear[] schoolYears)
        => this.Repository
            .Setup(repository => repository.SearchAsync(
                It.IsAny<ISpecification<SchoolYear>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<SchoolYear> specification, CancellationToken _)
                => InMemorySearch.Run(schoolYears, specification));

    protected void ShouldSearchNoSchoolYears()
        => this.ShouldSearchSchoolYears();

    protected void ShouldCaptureAddedSchoolYears()
        => this.Repository
            .Setup(repository => repository.AddAsync(It.IsAny<SchoolYear>(), It.IsAny<CancellationToken>()))
            .Callback<SchoolYear, CancellationToken>((schoolYear, _) => this._added.Add(schoolYear))
            .Returns(Task.CompletedTask);

    protected void ShouldNotHaveSearched()
        => this.Repository.Verify(
            repository => repository.SearchAsync(
                It.IsAny<ISpecification<SchoolYear>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldFindSchool(Guid schoolId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<SchoolEnsure.Query>(query => query.SchoolId == schoolId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

    protected void ShouldNotFindSchool(Guid schoolId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<SchoolEnsure.Query>(query => query.SchoolId == schoolId),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new SchoolExceptions.NotFound());

    protected void ShouldFindClassesOfSchoolYear(Guid schoolYearId, params Guid[] classIds)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<ClassFinderBySchoolYear.Query>(query => query.SchoolYearId == schoolYearId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(classIds.Select(classId => ClassResponseMother.With(classId, schoolYearId)).ToList());

    protected void ShouldAllowClassRepresentative(Guid userId, Guid classId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<UserIsClassRepresentative.Query>(query => query.UserId == userId && query.ClassId == classId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

    protected void ShouldDenyClassRepresentative(Guid userId, Guid classId)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<UserIsClassRepresentative.Query>(query => query.UserId == userId && query.ClassId == classId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
}
