namespace SumiClas.Tests.Localities.Application;

using AwesomeAssertions;
using SumiClas.Localities.Application;
using SumiClas.Tests.Localities.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class LocalityFinderByProvinceShould : LocalitiesUnitTestCase
{

    [Test]
    public async Task return_only_that_province_ordered_by_name_when_localities_are_requested_by_province()
    {
        Guid provinceId = GuidMother.Random();
        Guid otherProvinceId = GuidMother.Random();
        string firstName = StringMother.Random("alpha");
        string lastName = StringMother.Random("zeta");
        this.ShouldReturnLocalities(
            LocalityMother.InProvince(provinceId, lastName),
            LocalityMother.InProvince(provinceId, firstName),
            LocalityMother.InProvince(otherProvinceId, StringMother.Random("other")));

        IReadOnlyCollection<LocalityResponse> result = await this.NewLocalityFinderByProvince().ExecuteAsync(
            LocalityFinderByProvinceQueryMother.WithProvinceId(provinceId),
            CancellationToken.None);

        result.Select(locality => locality.Name).Should().Equal(firstName, lastName);
        result.Select(locality => locality.ProvinceId).Should().OnlyContain(id => id == provinceId);
    }

    [TestCase("nijar")]
    [TestCase("NÍJ")]
    [TestCase("  jar ")]
    public async Task match_the_name_ignoring_case_and_accents_when_localities_are_requested_with_a_name(string name)
    {
        Guid provinceId = GuidMother.Random();
        this.ShouldReturnLocalities(
            LocalityMother.InProvince(provinceId, "Níjar"),
            LocalityMother.InProvince(provinceId, StringMother.Random("other")));

        IReadOnlyCollection<LocalityResponse> result = await this.NewLocalityFinderByProvince().ExecuteAsync(
            LocalityFinderByProvinceQueryMother.WithName(provinceId, name),
            CancellationToken.None);

        result.Select(locality => locality.Name).Should().Equal("Níjar");
    }

    [Test]
    public async Task return_empty_when_localities_are_requested_for_an_unknown_province()
    {
        Guid provinceId = GuidMother.Random();
        this.ShouldReturnLocalities(LocalityMother.InProvince(provinceId, StringMother.Random("locality")));

        IReadOnlyCollection<LocalityResponse> result = await this.NewLocalityFinderByProvince().ExecuteAsync(
            LocalityFinderByProvinceQueryMother.WithProvinceId(GuidMother.Random()),
            CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Test]
    public async Task return_the_use_case_result_when_the_locality_finder_by_province_handler_handles_the_query()
    {
        Guid provinceId = GuidMother.Random();
        Guid otherProvinceId = GuidMother.Random();
        string name = StringMother.Random("locality");
        this.ShouldReturnLocalities(
            LocalityMother.InProvince(provinceId, name),
            LocalityMother.InProvince(otherProvinceId, StringMother.Random("other")));
        LocalityFinderByProvince.Handler handler = new(this.NewLocalityFinderByProvince());

        IReadOnlyCollection<LocalityResponse> result = await handler.Handle(
            LocalityFinderByProvinceQueryMother.WithProvinceId(provinceId),
            CancellationToken.None);

        result.Select(locality => locality.Name).Should().Equal(name);
    }

    private LocalityFinderByProvince.Finder NewLocalityFinderByProvince() => new(this.Repository.Object);
}
