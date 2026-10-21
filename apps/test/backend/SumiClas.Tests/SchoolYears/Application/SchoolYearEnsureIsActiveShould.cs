namespace SumiClas.Tests.SchoolYears.Application;

using AwesomeAssertions;
using SumiClas.SchoolYears.Application;
using SumiClas.SchoolYears.Domain;
using SumiClas.SchoolYears.Domain.Services;
using SumiClas.Shared.Domain;
using SumiClas.Tests.SchoolYears.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class SchoolYearEnsureIsActiveShould : SchoolYearsUnitTestCase
{
    private readonly SchoolYearEnsureIsActive.EnsureIsActive _useCase;

    public SchoolYearEnsureIsActiveShould()
    {
        this._useCase = new SchoolYearEnsureIsActive.EnsureIsActive(new SchoolYearFinder(this.Repository.Object));
    }

    [Test]
    public async Task succeed_when_the_school_year_is_active()
    {
        SchoolYear schoolYear = SchoolYearMother.Active(GuidMother.Random());
        this.ShouldFetchSchoolYears(schoolYear, SchoolYearMother.Archived(GuidMother.Random()));

        Func<Task> act = () => this._useCase.ExecuteAsync(
            SchoolYearEnsureIsActiveQueryMother.WithSchoolYearId(schoolYear.Id));

        await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task throw_archived_when_the_school_year_is_archived()
    {
        SchoolYear schoolYear = SchoolYearMother.Archived(GuidMother.Random());
        this.ShouldFetchSchoolYears(schoolYear, SchoolYearMother.Active(GuidMother.Random()));

        Func<Task> act = () => this._useCase.ExecuteAsync(
            SchoolYearEnsureIsActiveQueryMother.WithSchoolYearId(schoolYear.Id));

        await act.Should().ThrowAsync<SchoolYearExceptions.Archived>();
    }

    [Test]
    public async Task throw_not_found_when_the_school_year_does_not_exist()
    {
        this.ShouldFetchSchoolYears(SchoolYearMother.Active(GuidMother.Random()));

        Func<Task> act = () => this._useCase.ExecuteAsync(
            SchoolYearEnsureIsActiveQueryMother.WithSchoolYearId(GuidMother.Random()));

        await act.Should().ThrowAsync<SchoolYearExceptions.NotFound>();
    }
}
