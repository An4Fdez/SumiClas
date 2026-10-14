namespace SumiClas.Tests.Provinces.Application;

using AwesomeAssertions;
using SumiClas.Provinces.Application;
using SumiClas.Provinces.Domain;
using SumiClas.Tests.Provinces.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class ProvinceFinderByIdsShould : ProvincesUnitTestCase
{
    [Test]
    public async Task return_only_the_existing_provinces_when_provinces_are_requested_by_ids()
    {
        Province known = ProvinceMother.Random();
        this.ShouldSearchProvinces(known, ProvinceMother.Random());

        IReadOnlyCollection<ProvinceResponse> result = await this.NewProvinceFinderByIds().ExecuteAsync(
            ProvinceFinderByIdsQueryMother.WithIds(known.Id, GuidMother.Random()),
            CancellationToken.None);

        result.Select(province => province.Id).Should().Equal(known.Id);
    }

    private ProvinceFinderByIds.Finder NewProvinceFinderByIds() => new(this.Repository.Object);
}
