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
    private readonly List<School> _stored = [];

    protected Mock<ISchoolRepository> Repository { get; } = new();

    [SetUp]
    public void SchoolsSetUp()
    {
        this._stored.Clear();
        this.Repository.Reset();
        this.Repository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => this._stored.FirstOrDefault(school => school.Id == id));
        this.Repository
            .Setup(repository => repository.SearchAsync(
                It.IsAny<ISpecification<School>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<School> specification, CancellationToken _)
                => InMemorySearch.Run(this._stored, specification));
    }

    protected void ShouldReturn(params School[] schools) => this._stored.AddRange(schools);

    protected void ShouldResolve(
        IReadOnlyCollection<LocalityResponse> localities,
        IReadOnlyCollection<ProvinceResponse> provinces,
        IReadOnlyCollection<PostalCodeResponse> postalCodes)
    {
        this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<LocalityFinderByIds.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(localities);
        this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<ProvinceFinderByIds.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provinces);
        this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<PostalCodeFinderByIds.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(postalCodes);
    }

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
