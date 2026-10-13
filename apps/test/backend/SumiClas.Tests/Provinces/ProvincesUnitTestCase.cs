namespace SumiClas.Tests.Provinces;

using Moq;
using SumiClas.Provinces.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;

public abstract class ProvincesUnitTestCase : UnitTestCase
{
    protected Mock<IProvinceRepository> Repository { get; } = new();

    [SetUp]
    public void ProvincesSetUp()
    {
        this.Repository.Reset();
        this.ShouldReturnProvinces();
    }

    protected void ShouldReturnProvinces(params Province[] provinces)
        => this.Repository
            .Setup(repository => repository.SearchAsync(
                It.IsAny<ISpecification<Province>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<Province> specification, CancellationToken _)
                => InMemorySearch.Run(provinces, specification));
}
