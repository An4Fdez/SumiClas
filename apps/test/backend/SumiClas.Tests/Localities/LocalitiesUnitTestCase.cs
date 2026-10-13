namespace SumiClas.Tests.Localities;

using Moq;
using SumiClas.Localities.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;

public abstract class LocalitiesUnitTestCase : UnitTestCase
{
    protected Mock<ILocalityRepository> Repository { get; } = new();

    [SetUp]
    public void LocalitiesSetUp()
    {
        this.Repository.Reset();
        this.ShouldReturnLocalities();
    }

    protected void ShouldReturnLocalities(params Locality[] localities)
        => this.Repository
            .Setup(repository => repository.SearchAsync(
                It.IsAny<ISpecification<Locality>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<Locality> specification, CancellationToken _)
                => InMemorySearch.Run(localities, specification));
}
