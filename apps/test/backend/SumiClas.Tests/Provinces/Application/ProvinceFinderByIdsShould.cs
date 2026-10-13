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
        this.ShouldReturnProvinces(known, ProvinceMother.Random());

        IReadOnlyCollection<ProvinceResponse> result = await this.NewProvinceFinderByIds().ExecuteAsync(
            ProvinceFinderByIdsQueryMother.WithIds(known.Id, GuidMother.Random()),
            CancellationToken.None);

        result.Select(province => province.Id).Should().Equal(known.Id);
    }

    [Test]
    public async Task return_the_use_case_result_when_the_province_finder_by_ids_handler_handles_the_query()
    {
        Province known = ProvinceMother.Random();
        this.ShouldReturnProvinces(known);
        ProvinceFinderByIds.Handler handler = new(this.NewProvinceFinderByIds());

        IReadOnlyCollection<ProvinceResponse> result = await handler.Handle(
            ProvinceFinderByIdsQueryMother.WithIds(known.Id),
            CancellationToken.None);

        result.Select(province => province.Id).Should().Equal(known.Id);
    }

    private ProvinceFinderByIds.Finder NewProvinceFinderByIds() => new(this.Repository.Object);
}
