namespace SumiClas.Tests.SchoolYears.Application;

using AwesomeAssertions;
using SumiClas.Schools.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.SchoolYears.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class SchoolYearFinderBySchoolShould : SchoolYearsUnitTestCase
{
    private static readonly DateOnly OLDEST_START = new(2023, 9, 1);
    private static readonly DateOnly MIDDLE_START = new(2024, 9, 1);
    private static readonly DateOnly NEWEST_START = new(2025, 9, 1);

    private readonly SchoolYearFinderBySchool.Finder _useCase;

    public SchoolYearFinderBySchoolShould()
    {
        this._useCase = new SchoolYearFinderBySchool.Finder(this.Repository.Object, this.QueryBus.Object);
    }

    [Test]
    public async Task return_the_school_years_of_the_school_newest_first_with_their_status_name()
    {
        Guid schoolId = GuidMother.Random();
        this.ShouldFindSchool(schoolId);
        SchoolYear oldest = SchoolYearMother.Starting(schoolId, OLDEST_START, SchoolYearStatus.Archived);
        SchoolYear middle = SchoolYearMother.Starting(schoolId, MIDDLE_START, SchoolYearStatus.Archived);
        SchoolYear newest = SchoolYearMother.Starting(schoolId, NEWEST_START, SchoolYearStatus.Active);
        this.ShouldSearchSchoolYears(oldest, newest, middle);

        IReadOnlyCollection<SchoolYearResponse> result = await this._useCase.ExecuteAsync(
            SchoolYearFinderBySchoolQueryMother.WithSchoolId(schoolId));
        result.Select(response => response.Id).Should().Equal(newest.Id, middle.Id, oldest.Id);
        result.Select(response => response.Status).Should().Equal("Active", "Archived", "Archived");
    }

    [Test]
    public async Task return_the_data_of_each_school_year_of_the_school_in_the_response()
    {
        Guid schoolId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        string label = StringMother.Random("year");
        this.ShouldFindSchool(schoolId);
        this.ShouldSearchSchoolYears(
            SchoolYearMother.Detailed(
                schoolYearId,
                schoolId,
                label,
                MIDDLE_START,
                NEWEST_START,
                SchoolYearStatus.Active),
            SchoolYearMother.Starting(GuidMother.Random(), NEWEST_START, SchoolYearStatus.Active));

        IReadOnlyCollection<SchoolYearResponse> result = await this._useCase.ExecuteAsync(
            SchoolYearFinderBySchoolQueryMother.WithSchoolId(schoolId));

        SchoolYearResponse response = result.Single();
        response.Id.Should().Be(schoolYearId);
        response.SchoolId.Should().Be(schoolId);
        response.Label.Should().Be(label);
        response.StartDate.Should().Be(MIDDLE_START);
        response.EndDate.Should().Be(NEWEST_START);
    }

    [TestCase("active")]
    [TestCase("ACTIVE")]
    public async Task return_only_the_active_school_years_when_the_status_is_active_in_any_casing(string status)
    {
        Guid schoolId = GuidMother.Random();
        this.ShouldFindSchool(schoolId);
        SchoolYear active = SchoolYearMother.Starting(schoolId, NEWEST_START, SchoolYearStatus.Active);
        this.ShouldSearchSchoolYears(
            active,
            SchoolYearMother.Starting(schoolId, OLDEST_START, SchoolYearStatus.Archived));

        IReadOnlyCollection<SchoolYearResponse> result = await this._useCase.ExecuteAsync(
            SchoolYearFinderBySchoolQueryMother.With(schoolId, status));

        result.Select(response => response.Id).Should().Equal(active.Id);
    }

    [Test]
    public async Task return_only_the_archived_school_years_when_the_status_is_archived()
    {
        Guid schoolId = GuidMother.Random();
        this.ShouldFindSchool(schoolId);
        SchoolYear archived = SchoolYearMother.Starting(schoolId, OLDEST_START, SchoolYearStatus.Archived);
        this.ShouldSearchSchoolYears(
            archived,
            SchoolYearMother.Starting(schoolId, NEWEST_START, SchoolYearStatus.Active));

        IReadOnlyCollection<SchoolYearResponse> result = await this._useCase.ExecuteAsync(
            SchoolYearFinderBySchoolQueryMother.With(schoolId, "Archived"));

        result.Select(response => response.Id).Should().Equal(archived.Id);
    }

    [TestCase(null)]
    [TestCase("   ")]
    public async Task return_the_school_years_of_every_status_when_the_status_is_missing_or_blank(string? status)
    {
        Guid schoolId = GuidMother.Random();
        this.ShouldFindSchool(schoolId);
        SchoolYear active = SchoolYearMother.Starting(schoolId, NEWEST_START, SchoolYearStatus.Active);
        SchoolYear archived = SchoolYearMother.Starting(schoolId, OLDEST_START, SchoolYearStatus.Archived);
        this.ShouldSearchSchoolYears(archived, active);

        IReadOnlyCollection<SchoolYearResponse> result = await this._useCase.ExecuteAsync(
            SchoolYearFinderBySchoolQueryMother.With(schoolId, status));

        result.Select(response => response.Id).Should().Equal(active.Id, archived.Id);
    }

    [TestCase("Unknown")]
    [TestCase("7")]
    public async Task throw_invalid_status_without_asking_the_school_or_searching_when_the_status_is_not_valid(
        string status)
    {
        Func<Task> act = () => this._useCase.ExecuteAsync(
            SchoolYearFinderBySchoolQueryMother.With(GuidMother.Random(), status));

        await act.Should().ThrowAsync<SchoolYearExceptions.InvalidStatus>();
        this.QueryBus.VerifyNoOtherCalls();
        this.ShouldNotHaveSearched();
    }

    [Test]
    public async Task propagate_the_school_not_found_without_searching_when_the_school_does_not_exist()
    {
        Guid schoolId = GuidMother.Random();
        this.ShouldNotFindSchool(schoolId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            SchoolYearFinderBySchoolQueryMother.WithSchoolId(schoolId));

        await act.Should().ThrowAsync<SchoolExceptions.NotFound>();
        this.ShouldNotHaveSearched();
    }

    [Test]
    public async Task return_an_empty_list_when_the_school_has_no_school_years()
    {
        Guid schoolId = GuidMother.Random();
        this.ShouldFindSchool(schoolId);
        this.ShouldSearchNoSchoolYears();

        IReadOnlyCollection<SchoolYearResponse> result = await this._useCase.ExecuteAsync(
            SchoolYearFinderBySchoolQueryMother.WithSchoolId(schoolId));
        result.Should().BeEmpty();
    }
}
