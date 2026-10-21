namespace SumiClas.Tests.Schools.Application;

using AwesomeAssertions;
using SumiClas.Schools.Application;
using SumiClas.Schools.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Localities.Application;
using SumiClas.Tests.PostalCodes.Application;
using SumiClas.Tests.Provinces.Application;
using SumiClas.Tests.Schools.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class SchoolFinderByLocalityShould : SchoolsUnitTestCase
{
    private readonly SchoolFinderByLocality.Finder _useCase;

    public SchoolFinderByLocalityShould()
    {
        this._useCase = new SchoolFinderByLocality.Finder(this.Repository.Object, this.QueryBus.Object);
    }

    [Test]
    public async Task return_only_the_schools_of_that_locality()
    {
        Guid localityId = GuidMother.Random();
        Guid otherLocalityId = GuidMother.Random();
        string schoolName = StringMother.Random("school");
        Guid provinceId = GuidMother.Random();
        this.ShouldFindLocalities(
            [LocalityResponseMother.With(localityId, provinceId, StringMother.Random("locality"))]);
        this.ShouldFindProvinces([ProvinceResponseMother.With(provinceId, StringMother.Random("province"))]);
        School inMalaga = SchoolMother.InLocality(schoolName, localityId);
        this.ShouldSearchSchools(inMalaga, SchoolMother.InLocality(schoolName, otherLocalityId));

        IReadOnlyCollection<SchoolResponse> result = await this.Find(localityId);

        result.Select(school => school.Id).Should().Equal(inMalaga.Id);
        result.Single().LocalityId.Should().Be(localityId);
    }

    [Test]
    public async Task resolve_the_province_locality_and_postal_code_names()
    {
        Guid provinceId = GuidMother.Random();
        Guid localityId = GuidMother.Random();
        Guid postalCodeId = GuidMother.Random();
        string provinceName = StringMother.Random("province");
        string localityName = StringMother.Random("locality");
        string postalCode = StringMother.Random("postal");
        this.ShouldFindLocalities([LocalityResponseMother.With(localityId, provinceId, localityName)]);
        this.ShouldFindProvinces([ProvinceResponseMother.With(provinceId, provinceName)]);
        this.ShouldFindPostalCodes([PostalCodeResponseMother.With(postalCodeId, localityId, postalCode)]);
        this.ShouldSearchSchools(SchoolMother.InLocality(StringMother.Random("school"), localityId, postalCodeId));

        SchoolResponse school = (await this.Find(localityId)).Single();

        school.ProvinceId.Should().Be(provinceId);
        school.ProvinceName.Should().Be(provinceName);
        school.LocalityName.Should().Be(localityName);
        school.PostalCodeId.Should().Be(postalCodeId);
        school.PostalCode.Should().Be(postalCode);
    }

    [Test]
    public async Task return_a_null_code_and_not_ask_for_postal_codes_when_the_school_has_no_postal_code()
    {
        Guid localityId = GuidMother.Random();
        Guid provinceId = GuidMother.Random();
        this.ShouldFindLocalities(
            [LocalityResponseMother.With(localityId, provinceId, StringMother.Random("locality"))]);
        this.ShouldFindProvinces([ProvinceResponseMother.With(provinceId, StringMother.Random("province"))]);
        this.ShouldSearchSchools(SchoolMother.InLocality(StringMother.Random("school"), localityId));

        SchoolResponse school = (await this.Find(localityId)).Single();

        school.PostalCodeId.Should().BeNull();
        school.PostalCode.Should().BeNull();
        this.ShouldNotHaveAskedPostalCodes();
    }

    [Test]
    public async Task ask_each_module_only_once_when_there_are_many_schools()
    {
        Guid localityId = GuidMother.Random();
        Guid postalCodeId = GuidMother.Random();
        Guid provinceId = GuidMother.Random();
        this.ShouldFindLocalities(
            [LocalityResponseMother.With(localityId, provinceId, StringMother.Random("locality"))]);
        this.ShouldFindProvinces([ProvinceResponseMother.With(provinceId, StringMother.Random("province"))]);
        this.ShouldFindPostalCodes(
            [PostalCodeResponseMother.With(postalCodeId, localityId, StringMother.Random("postal"))]);
        this.ShouldSearchSchools(Enumerable.Range(0, 10)
            .Select(index => SchoolMother.InLocality(NameOf(index), localityId, postalCodeId))
            .ToArray());

        await this.Find(localityId);

        this.ShouldHaveAskedEachModuleOnce();
    }

    [Test]
    public async Task match_the_name_ignoring_case_and_accents_when_a_name_is_given()
    {
        Guid localityId = GuidMother.Random();
        Guid provinceId = GuidMother.Random();
        this.ShouldFindLocalities(
            [LocalityResponseMother.With(localityId, provinceId, StringMother.Random("locality"))]);
        this.ShouldFindProvinces([ProvinceResponseMother.With(provinceId, StringMother.Random("province"))]);
        School match = SchoolMother.InLocality("CEIP Peña Blanca", localityId);
        this.ShouldSearchSchools(match, SchoolMother.InLocality(StringMother.Random("other"), localityId));

        IReadOnlyCollection<SchoolResponse> result = await this.Find(localityId, "  PENA bla ");

        result.Select(school => school.Id).Should().Equal(match.Id);
    }

    [Test]
    public async Task match_the_unaccented_name_when_the_search_is_accented()
    {
        Guid localityId = GuidMother.Random();
        Guid provinceId = GuidMother.Random();
        this.ShouldFindLocalities(
            [LocalityResponseMother.With(localityId, provinceId, StringMother.Random("locality"))]);
        this.ShouldFindProvinces([ProvinceResponseMother.With(provinceId, StringMother.Random("province"))]);
        School match = SchoolMother.InLocality("CEIP Alamos", localityId);
        this.ShouldSearchSchools(match);

        IReadOnlyCollection<SchoolResponse> result = await this.Find(localityId, "Álamos");

        result.Select(school => school.Id).Should().Equal(match.Id);
    }

    [Test]
    public async Task return_every_school_of_the_locality_ordered_by_name_when_the_name_is_blank()
    {
        Guid localityId = GuidMother.Random();
        string firstName = StringMother.Random("alpha");
        string lastName = StringMother.Random("zeta");
        Guid provinceId = GuidMother.Random();
        this.ShouldFindLocalities(
            [LocalityResponseMother.With(localityId, provinceId, StringMother.Random("locality"))]);
        this.ShouldFindProvinces([ProvinceResponseMother.With(provinceId, StringMother.Random("province"))]);
        this.ShouldSearchSchools(
            SchoolMother.InLocality(lastName, localityId),
            SchoolMother.InLocality(firstName, localityId));

        IReadOnlyCollection<SchoolResponse> result = await this.Find(localityId, "  ");

        result.Select(school => school.Name).Should().Equal(firstName, lastName);
    }

    [Test]
    public async Task return_nothing_without_asking_other_modules_when_the_locality_is_unknown()
    {
        this.ShouldSearchSchools(SchoolMother.InLocality(StringMother.Random("school"), GuidMother.Random()));

        IReadOnlyCollection<SchoolResponse> result = await this.Find(GuidMother.Random());

        result.Should().BeEmpty();
        this.ShouldNotHaveAskedLocalities();
    }

    [Test]
    public async Task return_at_most_max_results_taking_the_first_by_name()
    {
        Guid localityId = GuidMother.Random();
        Guid provinceId = GuidMother.Random();
        this.ShouldFindLocalities(
            [LocalityResponseMother.With(localityId, provinceId, StringMother.Random("locality"))]);
        this.ShouldFindProvinces([ProvinceResponseMother.With(provinceId, StringMother.Random("province"))]);
        this.ShouldSearchSchools(Enumerable.Range(0, SchoolFinderByLocality.MaxResults + 5)
            .Select(index => SchoolMother.InLocality(NameOf(index), localityId))
            .ToArray());

        IReadOnlyCollection<SchoolResponse> result = await this.Find(localityId);

        result.Should().HaveCount(SchoolFinderByLocality.MaxResults);
        result.First().Name.Should().Be(NameOf(0));
        result.Last().Name.Should().Be(NameOf(SchoolFinderByLocality.MaxResults - 1));
    }

    [TestCase(null)]
    [TestCase("00000000-0000-0000-0000-000000000000")]
    public async Task fail_without_touching_the_repository_or_the_bus_when_there_is_no_locality(string? localityId)
    {
        Guid? id = localityId is null ? null : Guid.Parse(localityId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            SchoolFinderByLocalityQueryMother.WithoutLocality(id),
            CancellationToken.None);

        await act.Should().ThrowAsync<SchoolExceptions.LocalityFilterRequired>();
        this.ShouldNotHaveSearched();
        this.ShouldNotHaveAskedAnyModule();
    }

    private static string NameOf(int index) => $"school-{index:000}";

    private Task<IReadOnlyCollection<SchoolResponse>> Find(Guid localityId, string? name = null)
        => this._useCase.ExecuteAsync(
            SchoolFinderByLocalityQueryMother.With(localityId, name),
            CancellationToken.None);
}
