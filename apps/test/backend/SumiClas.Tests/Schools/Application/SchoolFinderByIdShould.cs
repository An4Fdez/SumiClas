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
    private readonly SchoolFinderById.Finder _useCase;

    public SchoolFinderByIdShould()
    {
        this._useCase = new SchoolFinderById.Finder(new SchoolFinder(this.Repository.Object), this.QueryBus.Object);
    }

    [Test]
    public async Task return_the_school_with_territory_names_and_phone_when_it_is_found()
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
        this.ShouldFindLocalities([LocalityResponseMother.With(localityId, provinceId, localityName)]);
        this.ShouldFindProvinces([ProvinceResponseMother.With(provinceId, provinceName)]);
        this.ShouldFindPostalCodes([PostalCodeResponseMother.With(postalCodeId, localityId, postalCode)]);
        School school = SchoolMother.WithContact(schoolName, localityId, postalCodeId, address, phone);
        this.ShouldFetchSchools(school);

        Result<SchoolResponse> result = await this._useCase.ExecuteAsync(
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
    public async Task return_not_found_without_asking_other_modules_when_the_school_is_unknown()
    {
        this.ShouldFetchNoSchools();

        Result<SchoolResponse> result = await this._useCase.ExecuteAsync(
            SchoolFinderByIdQueryMother.Random(),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SchoolErrors.NotFound);
        this.ShouldNotHaveAskedLocalities();
    }

    [Test]
    public async Task fail_loudly_when_the_locality_is_missing_from_the_catalog()
    {
        Guid provinceId = GuidMother.Random();
        this.ShouldFindLocalities(
            [LocalityResponseMother.With(GuidMother.Random(), provinceId, StringMother.Random("locality"))]);
        this.ShouldFindProvinces([ProvinceResponseMother.With(provinceId, StringMother.Random("province"))]);
        School school = SchoolMother.InLocality(StringMother.Random("school"), GuidMother.Random());
        this.ShouldFetchSchools(school);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            SchoolFinderByIdQueryMother.WithSchoolId(school.Id),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
