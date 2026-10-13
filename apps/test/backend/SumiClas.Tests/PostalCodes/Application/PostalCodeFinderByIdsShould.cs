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
        this.ShouldReturnPostalCodes(postalCode);

        IReadOnlyCollection<PostalCodeResponse> result = await this.NewPostalCodeFinderByIds().ExecuteAsync(
            PostalCodeFinderByIdsQueryMother.WithIds(postalCode.Id, GuidMother.Random()),
            CancellationToken.None);

        result.Single().Code.Should().Be(postalCode.Code);
        result.Single().LocalityId.Should().Be(localityId);
    }

    [Test]
    public async Task return_the_use_case_result_when_the_postal_code_finder_by_ids_handler_handles_the_query()
    {
        PostalCode postalCode = PostalCodeMother.Random();
        this.ShouldReturnPostalCodes(postalCode);
        PostalCodeFinderByIds.Handler handler = new(this.NewPostalCodeFinderByIds());

        IReadOnlyCollection<PostalCodeResponse> result = await handler.Handle(
            PostalCodeFinderByIdsQueryMother.WithIds(postalCode.Id),
            CancellationToken.None);

        result.Single().Code.Should().Be(postalCode.Code);
    }

    private PostalCodeFinderByIds.Finder NewPostalCodeFinderByIds() => new(this.Repository.Object);
}
