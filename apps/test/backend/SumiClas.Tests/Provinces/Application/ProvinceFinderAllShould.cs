namespace SumiClas.Tests.Provinces.Application;

using AwesomeAssertions;
using SumiClas.Provinces.Application;
using SumiClas.Tests.Provinces.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class ProvinceFinderAllShould : ProvincesUnitTestCase
{
    [Test]
    public async Task return_every_province_ordered_by_name_ignoring_accents_when_all_provinces_are_requested()
    {
        string accentedFirst = "Ála-" + StringMother.Random("x");
        string plainSecond = "Alm-" + StringMother.Random("x");
        string plainLast = "Sev-" + StringMother.Random("x");
        this.ShouldSearchProvinces(
            ProvinceMother.WithName(plainLast),
            ProvinceMother.WithName(accentedFirst),
            ProvinceMother.WithName(plainSecond));

        IReadOnlyCollection<ProvinceResponse> result = await this.NewProvinceFinderAll()
            .ExecuteAsync(new ProvinceFinderAll.Query(), CancellationToken.None);

        result.Select(province => province.Name).Should().Equal(accentedFirst, plainSecond, plainLast);
    }

    private ProvinceFinderAll.Finder NewProvinceFinderAll() => new(this.Repository.Object);
}
