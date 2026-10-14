namespace SumiClas.Tests.PostalCodes;

using Moq;
using SumiClas.PostalCodes.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;

public abstract class PostalCodesUnitTestCase : UnitTestCase
{
    protected Mock<IPostalCodeRepository> Repository { get; } = new();

    protected void ShouldSearchPostalCodes(params PostalCode[] postalCodes)
        => this.Repository
            .Setup(repository => repository.SearchAsync(
                It.IsAny<ISpecification<PostalCode>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<PostalCode> specification, CancellationToken _)
                => InMemorySearch.Run(postalCodes, specification));
}
