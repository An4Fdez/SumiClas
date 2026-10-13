namespace SumiClas.Tests.Provinces.Application;

using AwesomeAssertions;
using SumiClas.Provinces.Application;
using SumiClas.Provinces.Domain;
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
        this.ShouldReturnProvinces(
            ProvinceMother.WithName(plainLast),
            ProvinceMother.WithName(accentedFirst),
            ProvinceMother.WithName(plainSecond));

        IReadOnlyCollection<ProvinceResponse> result = await this.NewProvinceFinderAll()
            .ExecuteAsync(new ProvinceFinderAll.Query(), CancellationToken.None);

        result.Select(province => province.Name).Should().Equal(accentedFirst, plainSecond, plainLast);
    }

    [Test]
    public async Task return_the_use_case_result_when_the_province_finder_all_handler_handles_the_query()
    {
        Province known = ProvinceMother.Random();
        this.ShouldReturnProvinces(known);
        ProvinceFinderAll.Handler handler = new(this.NewProvinceFinderAll());

        IReadOnlyCollection<ProvinceResponse> result = await handler.Handle(
            new ProvinceFinderAll.Query(),
            CancellationToken.None);

        result.Select(province => province.Id).Should().Equal(known.Id);
    }

    private ProvinceFinderAll.Finder NewProvinceFinderAll() => new(this.Repository.Object);
}
