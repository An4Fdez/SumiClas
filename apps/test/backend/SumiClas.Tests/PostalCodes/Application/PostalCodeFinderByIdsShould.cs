namespace SumiClas.Tests.PostalCodes.Application;

using AwesomeAssertions;
using SumiClas.PostalCodes.Application;
using SumiClas.PostalCodes.Domain;
using SumiClas.Tests.PostalCodes.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class PostalCodeFinderByIdsShould : PostalCodesUnitTestCase
{
    [Test]
    public async Task return_the_five_digit_code_when_postal_codes_are_requested_by_ids()
    {
        Guid localityId = GuidMother.Random();
        PostalCode postalCode = PostalCodeMother.With(localityId: localityId);
        this.ShouldSearchPostalCodes(postalCode);

        IReadOnlyCollection<PostalCodeResponse> result = await this.NewPostalCodeFinderByIds().ExecuteAsync(
            PostalCodeFinderByIdsQueryMother.WithIds(postalCode.Id, GuidMother.Random()),
            CancellationToken.None);

        result.Single().Code.Should().Be(postalCode.Code);
        result.Single().LocalityId.Should().Be(localityId);
    }

    private PostalCodeFinderByIds.Finder NewPostalCodeFinderByIds() => new(this.Repository.Object);
}
