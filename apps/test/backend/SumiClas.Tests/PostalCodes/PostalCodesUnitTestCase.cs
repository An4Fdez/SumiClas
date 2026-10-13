namespace SumiClas.Tests.PostalCodes;

using Moq;
using SumiClas.PostalCodes.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;

public abstract class PostalCodesUnitTestCase : UnitTestCase
{
    protected Mock<IPostalCodeRepository> Repository { get; } = new();

    [SetUp]
    public void PostalCodesSetUp()
    {
        this.Repository.Reset();
        this.ShouldReturnPostalCodes();
    }

    protected void ShouldReturnPostalCodes(params PostalCode[] postalCodes)
        => this.Repository
            .Setup(repository => repository.SearchAsync(
                It.IsAny<ISpecification<PostalCode>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<PostalCode> specification, CancellationToken _)
                => InMemorySearch.Run(postalCodes, specification));
}
