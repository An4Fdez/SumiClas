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
        this.ShouldSearchLocalities(
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
        this.ShouldSearchLocalities(
            LocalityMother.InProvince(provinceId, "Níjar"),
            LocalityMother.InProvince(provinceId, StringMother.Random("other")));

        IReadOnlyCollection<LocalityResponse> result = await this.NewLocalityFinderByProvince().ExecuteAsync(
            LocalityFinderByProvinceQueryMother.WithName(provinceId, name),
            CancellationToken.None);

        result.Select(locality => locality.Name).Should().Equal("Níjar");
    }

    private LocalityFinderByProvince.Finder NewLocalityFinderByProvince() => new(this.Repository.Object);
}
