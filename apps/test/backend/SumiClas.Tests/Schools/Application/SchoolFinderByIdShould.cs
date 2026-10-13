namespace SumiClas.Tests.Schools.Application;

using AwesomeAssertions;
using SumiClas.Schools.Application;
using SumiClas.Schools.Domain;
using SumiClas.Schools.Domain.Services;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Localities.Application;
using SumiClas.Tests.PostalCodes.Application;
using SumiClas.Tests.Provinces.Application;
using SumiClas.Tests.Schools.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class SchoolFinderByIdShould : SchoolsUnitTestCase
{
    private readonly SchoolFinderById.Finder _byIdFinder;
    private readonly SchoolFinderByIds.Finder _byIdsFinder;

    public SchoolFinderByIdShould()
    {
        this._byIdFinder = new SchoolFinderById.Finder(new SchoolFinder(this.Repository.Object), this.QueryBus.Object);
        this._byIdsFinder = new SchoolFinderByIds.Finder(this.Repository.Object, this.QueryBus.Object);
    }

    [Test]
    public async Task return_the_school_with_territory_names_and_phone_when_it_is_found_by_id()
    {
        Guid provinceId = GuidMother.Random();
        Guid localityId = GuidMother.Random();
        Guid postalCodeId = GuidMother.Random();
        string provinceName = StringMother.Random("province");
        string localityName = StringMother.Random("locality");
        string postalCode = StringMother.Random("postal");
        string schoolName = StringMother.Random("school");
        string address = StringMother.Random("address");
        string phone = StringMother.Random("phone");
        this.ShouldResolve(
            [LocalityResponseMother.With(localityId, provinceId, localityName)],
            [ProvinceResponseMother.With(provinceId, provinceName)],
            [PostalCodeResponseMother.With(postalCodeId, localityId, postalCode)]);
        School school = SchoolMother.WithContact(schoolName, localityId, postalCodeId, address, phone);
        this.ShouldReturn(school);

        Result<SchoolResponse> result = await this._byIdFinder.ExecuteAsync(
            SchoolFinderByIdQueryMother.WithSchoolId(school.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        SchoolResponse response = result.Value;
        response.Name.Should().Be(schoolName);
        response.ProvinceName.Should().Be(provinceName);
        response.LocalityName.Should().Be(localityName);
        response.PostalCode.Should().Be(postalCode);
        response.Address.Should().Be(address);
        response.Phone.Should().Be(phone);
    }

    [Test]
    public async Task return_not_found_without_asking_other_modules_when_the_school_is_unknown_by_id()
    {
        Result<SchoolResponse> result = await this._byIdFinder.ExecuteAsync(
            SchoolFinderByIdQueryMother.Random(),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SchoolErrors.NotFound);
        this.ShouldNotHaveAskedLocalities();
    }

    [Test]
    public async Task resolve_schools_of_different_provinces_with_one_query_per_module_when_found_by_ids()
    {
        Guid provinceId = GuidMother.Random();
        Guid otherProvinceId = GuidMother.Random();
        Guid localityId = GuidMother.Random();
        Guid otherLocalityId = GuidMother.Random();
        Guid postalCodeId = GuidMother.Random();
        string provinceName = StringMother.Random("province");
        string otherProvinceName = StringMother.Random("other-province");
        this.ShouldResolve(
            [
                LocalityResponseMother.With(localityId, provinceId, StringMother.Random("locality")),
                LocalityResponseMother.With(otherLocalityId, otherProvinceId, StringMother.Random("other-locality")),
            ],
            [
                ProvinceResponseMother.With(provinceId, provinceName),
                ProvinceResponseMother.With(otherProvinceId, otherProvinceName),
            ],
            [PostalCodeResponseMother.With(postalCodeId, localityId, StringMother.Random("postal"))]);
        School first = SchoolMother.InLocality(StringMother.Random("school"), localityId, postalCodeId);
        School second = SchoolMother.InLocality(StringMother.Random("other-school"), otherLocalityId);
        this.ShouldReturn(first, second);

        IReadOnlyCollection<SchoolResponse> result = await this._byIdsFinder.ExecuteAsync(
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
        IReadOnlyCollection<SchoolResponse> result = await this._byIdsFinder.ExecuteAsync(
            SchoolFinderByIdsQueryMother.Random(),
            CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Test]
    public async Task fail_loudly_when_the_locality_is_missing_from_the_catalog()
    {
        Guid provinceId = GuidMother.Random();
        this.ShouldResolve(
            [LocalityResponseMother.With(GuidMother.Random(), provinceId, StringMother.Random("locality"))],
            [ProvinceResponseMother.With(provinceId, StringMother.Random("province"))],
            []);
        School school = SchoolMother.InLocality(StringMother.Random("school"), GuidMother.Random());
        this.ShouldReturn(school);

        Func<Task> act = () => this._byIdFinder.ExecuteAsync(
            SchoolFinderByIdQueryMother.WithSchoolId(school.Id),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task return_the_use_case_result_when_the_school_finder_by_id_handler_handles_the_query()
    {
        SchoolFinderById.Handler handler = new(this._byIdFinder);

        Result<SchoolResponse> result = await handler.Handle(
            SchoolFinderByIdQueryMother.Random(),
            CancellationToken.None);

        result.Error.Should().Be(SchoolErrors.NotFound);
    }

    [Test]
    public async Task return_the_use_case_result_when_the_school_finder_by_ids_handler_handles_the_query()
    {
        SchoolFinderByIds.Handler handler = new(this._byIdsFinder);

        IReadOnlyCollection<SchoolResponse> result = await handler.Handle(
            SchoolFinderByIdsQueryMother.Random(),
            CancellationToken.None);

        result.Should().BeEmpty();
    }
}
