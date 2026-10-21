namespace SumiClas.Tests.SchoolYears.Application;

using AwesomeAssertions;
using SumiClas.SchoolYears.Application;
using SumiClas.SchoolYears.Domain;
using SumiClas.SchoolYears.Domain.Services;
using SumiClas.Shared.Domain;
using SumiClas.Tests.SchoolYears.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class SchoolIdFinderBySchoolYearShould : SchoolYearsUnitTestCase
{
    private readonly SchoolIdFinderBySchoolYear.Finder _useCase;

    public SchoolIdFinderBySchoolYearShould()
    {
        this._useCase = new SchoolIdFinderBySchoolYear.Finder(new SchoolYearFinder(this.Repository.Object));
    }

    [Test]
    public async Task return_the_school_of_the_requested_school_year_only()
    {
        Guid schoolId = GuidMother.Random();
        SchoolYear requested = SchoolYearMother.OfSchool(schoolId);
        SchoolYear other = SchoolYearMother.Active(GuidMother.Random());
        this.ShouldFetchSchoolYears(other, requested);

        Guid result = await this._useCase.ExecuteAsync(
            SchoolIdFinderBySchoolYearQueryMother.WithSchoolYearId(requested.Id));

        result.Should().Be(schoolId);
    }

    [Test]
    public async Task throw_not_found_when_the_school_year_does_not_exist()
    {
        this.ShouldFetchSchoolYears(SchoolYearMother.Active(GuidMother.Random()));

        Func<Task> act = () => this._useCase.ExecuteAsync(
            SchoolIdFinderBySchoolYearQueryMother.WithSchoolYearId(GuidMother.Random()));

        await act.Should().ThrowAsync<SchoolYearExceptions.NotFound>();
    }
}
