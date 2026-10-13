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
    private readonly SchoolYearEnsureIsActive.Handler _handler;

    public SchoolYearEnsureIsActiveShould()
    {
        this._useCase = new SchoolYearEnsureIsActive.EnsureIsActive(new SchoolYearFinder(this.Repository.Object));
        this._handler = new SchoolYearEnsureIsActive.Handler(this._useCase);
    }

    [Test]
    public async Task succeed_when_the_school_year_is_active()
    {
        SchoolYear schoolYear = SchoolYearMother.Active(GuidMother.Random());
        this.ShouldReturn(schoolYear, SchoolYearMother.Archived(GuidMother.Random()));

        Result result = await this._useCase.ExecuteAsync(
            SchoolYearEnsureIsActiveQueryMother.WithSchoolYearId(schoolYear.Id));

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task return_archived_when_the_school_year_is_archived()
    {
        SchoolYear schoolYear = SchoolYearMother.Archived(GuidMother.Random());
        this.ShouldReturn(schoolYear, SchoolYearMother.Active(GuidMother.Random()));

        Result result = await this._useCase.ExecuteAsync(
            SchoolYearEnsureIsActiveQueryMother.WithSchoolYearId(schoolYear.Id));

        result.Error.Should().Be(SchoolYearErrors.Archived);
    }

    [Test]
    public async Task return_not_found_when_the_school_year_does_not_exist()
    {
        this.ShouldReturn(SchoolYearMother.Active(GuidMother.Random()));

        Result result = await this._useCase.ExecuteAsync(
            SchoolYearEnsureIsActiveQueryMother.WithSchoolYearId(GuidMother.Random()));

        result.Error.Should().Be(SchoolYearErrors.NotFound);
    }

    [Test]
    public async Task return_the_use_case_result_when_the_handler_handles_the_query()
    {
        SchoolYear schoolYear = SchoolYearMother.Archived(GuidMother.Random());
        this.ShouldReturn(schoolYear);

        Result result = await this._handler.Handle(
            SchoolYearEnsureIsActiveQueryMother.WithSchoolYearId(schoolYear.Id),
            CancellationToken.None);

        result.Error.Should().Be(SchoolYearErrors.Archived);
    }
}
