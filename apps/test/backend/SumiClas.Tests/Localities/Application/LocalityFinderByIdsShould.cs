namespace SumiClas.Tests.Localities.Application;

using AwesomeAssertions;
using SumiClas.Localities.Application;
using SumiClas.Localities.Domain;
using SumiClas.Tests.Localities.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class LocalityFinderByIdsShould : LocalitiesUnitTestCase
{

    [Test]
    public async Task return_only_the_existing_localities_with_their_province_when_requested_by_ids()
    {
        Guid provinceId = GuidMother.Random();
        Guid otherProvinceId = GuidMother.Random();
        Locality locality = LocalityMother.InProvince(provinceId, StringMother.Random("locality"));
        this.ShouldReturnLocalities(
            locality,
            LocalityMother.InProvince(otherProvinceId, StringMother.Random("other")));

        IReadOnlyCollection<LocalityResponse> result = await this.NewLocalityFinderByIds().ExecuteAsync(
            LocalityFinderByIdsQueryMother.WithIds(locality.Id, GuidMother.Random()),
            CancellationToken.None);

        result.Should().HaveCount(1);
        result.Single().ProvinceId.Should().Be(provinceId);
    }

    [Test]
    public async Task return_the_use_case_result_when_the_locality_finder_by_ids_handler_handles_the_query()
    {
        Guid provinceId = GuidMother.Random();
        Locality locality = LocalityMother.InProvince(provinceId, StringMother.Random("locality"));
        this.ShouldReturnLocalities(locality);
        LocalityFinderByIds.Handler handler = new(this.NewLocalityFinderByIds());

        IReadOnlyCollection<LocalityResponse> result = await handler.Handle(
            LocalityFinderByIdsQueryMother.WithIds(locality.Id),
            CancellationToken.None);

        result.Single().Id.Should().Be(locality.Id);
    }

    private LocalityFinderByIds.Finder NewLocalityFinderByIds() => new(this.Repository.Object);
}
