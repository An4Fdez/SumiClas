namespace SumiClas.Tests.Schools;

using Moq;
using SumiClas.Localities.Application;
using SumiClas.PostalCodes.Application;
using SumiClas.Provinces.Application;
using SumiClas.Schools.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;

public abstract class SchoolsUnitTestCase : UnitTestCase
{
    protected Mock<ISchoolRepository> Repository { get; } = new();

    [SetUp]
    public void SchoolsSetUp()
    {
        this.Repository.Reset();
    }

    protected void ShouldFetchSchools(params School[] schools)
        => this.Repository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => schools.FirstOrDefault(school => school.Id == id));

    protected void ShouldSearchSchools(params School[] schools)
        => this.Repository
            .Setup(repository => repository.SearchAsync(
                It.IsAny<ISpecification<School>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<School> specification, CancellationToken _)
                => InMemorySearch.Run(schools, specification));

    protected void ShouldFetchNoSchools()
        => this.ShouldFetchSchools();

    protected void ShouldFindLocalities(IReadOnlyCollection<LocalityResponse> localities)
        => this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<LocalityFinderByIds.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(localities);

    protected void ShouldFindProvinces(IReadOnlyCollection<ProvinceResponse> provinces)
        => this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<ProvinceFinderByIds.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provinces);

    protected void ShouldFindPostalCodes(IReadOnlyCollection<PostalCodeResponse> postalCodes)
        => this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<PostalCodeFinderByIds.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(postalCodes);

    protected void ShouldHaveAskedEachModuleOnce()
    {
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<LocalityFinderByIds.Query>(), It.IsAny<CancellationToken>()),
            Times.Once);
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<ProvinceFinderByIds.Query>(), It.IsAny<CancellationToken>()),
            Times.Once);
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<PostalCodeFinderByIds.Query>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    protected void ShouldNotHaveAskedLocalities()
        => this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<LocalityFinderByIds.Query>(), It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldNotHaveAskedPostalCodes()
        => this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<PostalCodeFinderByIds.Query>(), It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldNotHaveAskedAnyModule() => this.QueryBus.VerifyNoOtherCalls();

    protected void ShouldNotHaveSearched()
        => this.Repository.Verify(
            repository => repository.SearchAsync(
                It.IsAny<ISpecification<School>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
}
