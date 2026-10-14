namespace SumiClas.Tests.Localities;

using Moq;
using SumiClas.Localities.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;

public abstract class LocalitiesUnitTestCase : UnitTestCase
{
    protected Mock<ILocalityRepository> Repository { get; } = new();

    protected void ShouldSearchLocalities(params Locality[] localities)
        => this.Repository
            .Setup(repository => repository.SearchAsync(
                It.IsAny<ISpecification<Locality>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<Locality> specification, CancellationToken _)
                => InMemorySearch.Run(localities, specification));
}
