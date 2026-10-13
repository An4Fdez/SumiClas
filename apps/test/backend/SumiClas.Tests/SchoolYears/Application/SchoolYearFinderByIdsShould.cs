namespace SumiClas.Tests.SchoolYears.Application;

using AwesomeAssertions;
using SumiClas.SchoolYears.Application;
using SumiClas.SchoolYears.Domain;
using SumiClas.Tests.SchoolYears.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class SchoolYearFinderByIdsShould : SchoolYearsUnitTestCase
{
    private static readonly DateOnly START_DATE = new(2026, 9, 1);
    private static readonly DateOnly END_DATE = new(2027, 6, 30);

    private readonly SchoolYearFinderByIds.Finder _useCase;

    public SchoolYearFinderByIdsShould()
    {
        this._useCase = new SchoolYearFinderByIds.Finder(this.Repository.Object);
    }

    [Test]
    public async Task return_the_school_year_with_all_its_data_and_the_status_name()
    {
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        string label = StringMother.Random("year");
        this.ShouldReturn(SchoolYearMother.Detailed(
            schoolYearId,
            schoolId,
            label,
            START_DATE,
            END_DATE,
            SchoolYearStatus.Archived));

        IReadOnlyCollection<SchoolYearResponse> result = await this._useCase.ExecuteAsync(
            SchoolYearFinderByIdsQueryMother.WithIds(schoolYearId));

        SchoolYearResponse response = result.Single();
        response.Id.Should().Be(schoolYearId);
        response.SchoolId.Should().Be(schoolId);
        response.Label.Should().Be(label);
        response.StartDate.Should().Be(START_DATE);
        response.EndDate.Should().Be(END_DATE);
        response.Status.Should().Be("Archived");
    }

    [Test]
    public async Task return_the_active_status_name_for_an_active_school_year()
    {
        SchoolYear schoolYear = SchoolYearMother.Active(GuidMother.Random());
        this.ShouldReturn(schoolYear);

        IReadOnlyCollection<SchoolYearResponse> result = await this._useCase.ExecuteAsync(
            SchoolYearFinderByIdsQueryMother.WithIds(schoolYear.Id));

        result.Single().Status.Should().Be("Active");
    }

    [Test]
    public async Task return_only_the_requested_school_years_and_skip_unknown_ids()
    {
        SchoolYear requested = SchoolYearMother.Active(GuidMother.Random());
        SchoolYear otherRequested = SchoolYearMother.Archived(GuidMother.Random());
        this.ShouldReturn(requested, otherRequested, SchoolYearMother.Active(GuidMother.Random()));

        IReadOnlyCollection<SchoolYearResponse> result = await this._useCase.ExecuteAsync(
            SchoolYearFinderByIdsQueryMother.WithIds(requested.Id, GuidMother.Random(), otherRequested.Id));

        result.Select(response => response.Id).Should().BeEquivalentTo([requested.Id, otherRequested.Id]);
    }

    [Test]
    public async Task return_empty_when_no_school_year_matches_the_ids()
    {
        this.ShouldReturn(SchoolYearMother.Active(GuidMother.Random()));

        IReadOnlyCollection<SchoolYearResponse> result = await this._useCase.ExecuteAsync(
            SchoolYearFinderByIdsQueryMother.WithIds(GuidMother.Random()));

        result.Should().BeEmpty();
    }

    [Test]
    public async Task return_empty_when_no_ids_are_requested()
    {
        this.ShouldReturn(SchoolYearMother.Active(GuidMother.Random()));

        IReadOnlyCollection<SchoolYearResponse> result = await this._useCase.ExecuteAsync(
            SchoolYearFinderByIdsQueryMother.WithIds());

        result.Should().BeEmpty();
    }
}
