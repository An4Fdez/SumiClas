namespace SumiClas.Tests.Schools.Application;

using AwesomeAssertions;
using SumiClas.Schools.Application;
using SumiClas.Schools.Domain;
using SumiClas.Tests.Localities.Application;
using SumiClas.Tests.PostalCodes.Application;
using SumiClas.Tests.Provinces.Application;
using SumiClas.Tests.Schools.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class SchoolFinderByIdsShould : SchoolsUnitTestCase
{
    private readonly SchoolFinderByIds.Finder _useCase;

    public SchoolFinderByIdsShould()
    {
        this._useCase = new SchoolFinderByIds.Finder(this.Repository.Object, this.QueryBus.Object);
    }

    [Test]
    public async Task resolve_schools_of_different_provinces_with_one_query_per_module()
    {
        Guid provinceId = GuidMother.Random();
        Guid otherProvinceId = GuidMother.Random();
        Guid localityId = GuidMother.Random();
        Guid otherLocalityId = GuidMother.Random();
        Guid postalCodeId = GuidMother.Random();
        string provinceName = StringMother.Random("province");
        string otherProvinceName = StringMother.Random("other-province");
        this.ShouldFindLocalities(
            [
                LocalityResponseMother.With(localityId, provinceId, StringMother.Random("locality")),
                LocalityResponseMother.With(otherLocalityId, otherProvinceId, StringMother.Random("other-locality")),
            ]);
        this.ShouldFindProvinces(
            [
                ProvinceResponseMother.With(provinceId, provinceName),
                ProvinceResponseMother.With(otherProvinceId, otherProvinceName),
            ]);
        this.ShouldFindPostalCodes(
            [PostalCodeResponseMother.With(postalCodeId, localityId, StringMother.Random("postal"))]);
        School first = SchoolMother.InLocality(StringMother.Random("school"), localityId, postalCodeId);
        School second = SchoolMother.InLocality(StringMother.Random("other-school"), otherLocalityId);
        this.ShouldSearchSchools(first, second);

        IReadOnlyCollection<SchoolResponse> result = await this._useCase.ExecuteAsync(
            SchoolFinderByIdsQueryMother.WithIds(first.Id, second.Id),
            CancellationToken.None);

        result.Select(school => school.ProvinceName)
            .Should().BeEquivalentTo(provinceName, otherProvinceName);
        result.Single(school => school.Id == second.Id).PostalCode.Should().BeNull();
        this.ShouldHaveAskedEachModuleOnce();
    }

    [Test]
    public async Task return_empty_when_no_school_matches_the_ids()
    {
        this.ShouldSearchNoSchools();

        IReadOnlyCollection<SchoolResponse> result = await this._useCase.ExecuteAsync(
            SchoolFinderByIdsQueryMother.Random(),
            CancellationToken.None);

        result.Should().BeEmpty();
    }
}
