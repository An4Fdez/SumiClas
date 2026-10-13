namespace SumiClas.Tests.SchoolYears.Application;

using AwesomeAssertions;
using SumiClas.Schools.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Schools;
using SumiClas.Tests.Shared;

[TestFixture]
public class SchoolYearCreateShould : SchoolYearsUnitTestCase
{
    private static readonly DateOnly START_DATE = new(2026, 9, 1);
    private static readonly DateOnly END_DATE = new(2027, 6, 30);

    private readonly SchoolYearCreate.Create _useCase;

    public SchoolYearCreateShould()
    {
        this._useCase = new SchoolYearCreate.Create(this.Repository.Object, this.QueryBus.Object);
    }

    [Test]
    public async Task add_an_active_school_year_with_its_data_and_the_trimmed_label_when_the_school_exists()
    {
        Guid schoolYearId = GuidMother.Random();
        Guid schoolId = GuidMother.Random();
        string label = StringMother.Random("year");
        this.QueryBus.ShouldFindSchool(schoolId);

        Result result = await this._useCase.ExecuteAsync(
            SchoolYearCreateCommandMother.With(schoolYearId, schoolId, "  " + label + " ", START_DATE, END_DATE));

        result.IsSuccess.Should().BeTrue();
        SchoolYear added = this.Added.Single();
        added.Id.Should().Be(schoolYearId);
        added.SchoolId.Should().Be(schoolId);
        added.Label.Should().Be(label);
        added.StartDate.Should().Be(START_DATE);
        added.EndDate.Should().Be(END_DATE);
        added.Status.Should().Be(SchoolYearStatus.Active);
    }

    [Test]
    public async Task return_the_school_error_without_adding_when_the_school_does_not_exist()
    {
        Guid schoolId = GuidMother.Random();
        this.QueryBus.ShouldNotFindSchool(schoolId);

        Result result = await this._useCase.ExecuteAsync(SchoolYearCreateCommandMother.With(
            GuidMother.Random(),
            schoolId,
            StringMother.Random("year"),
            START_DATE,
            END_DATE));

        result.Error.Should().Be(SchoolErrors.NotFound);
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task return_the_school_error_before_validating_the_data_when_both_fail()
    {
        Guid schoolId = GuidMother.Random();
        this.QueryBus.ShouldNotFindSchool(schoolId);

        Result result = await this._useCase.ExecuteAsync(
            SchoolYearCreateCommandMother.With(Guid.Empty, schoolId, " ", END_DATE, START_DATE));

        result.Error.Should().Be(SchoolErrors.NotFound);
    }

    [Test]
    public async Task return_id_required_without_adding_when_the_school_year_id_is_empty()
    {
        Guid schoolId = GuidMother.Random();
        this.QueryBus.ShouldFindSchool(schoolId);

        Result result = await this._useCase.ExecuteAsync(SchoolYearCreateCommandMother.With(
            Guid.Empty,
            schoolId,
            StringMother.Random("year"),
            START_DATE,
            END_DATE));

        result.Error.Should().Be(SchoolYearErrors.IdRequired);
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task return_school_required_without_adding_when_the_school_id_is_empty()
    {
        this.QueryBus.ShouldFindSchool(Guid.Empty);

        Result result = await this._useCase.ExecuteAsync(SchoolYearCreateCommandMother.With(
            GuidMother.Random(),
            Guid.Empty,
            StringMother.Random("year"),
            START_DATE,
            END_DATE));

        result.Error.Should().Be(SchoolYearErrors.SchoolRequired);
        this.Added.Should().BeEmpty();
    }

    [TestCase("")]
    [TestCase("   ")]
    public async Task return_label_required_without_adding_when_the_label_is_blank(string label)
    {
        Guid schoolId = GuidMother.Random();
        this.QueryBus.ShouldFindSchool(schoolId);

        Result result = await this._useCase.ExecuteAsync(
            SchoolYearCreateCommandMother.With(GuidMother.Random(), schoolId, label, START_DATE, END_DATE));

        result.Error.Should().Be(SchoolYearErrors.LabelRequired);
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task return_invalid_date_range_without_adding_when_the_end_date_equals_the_start_date()
    {
        Guid schoolId = GuidMother.Random();
        this.QueryBus.ShouldFindSchool(schoolId);

        Result result = await this._useCase.ExecuteAsync(SchoolYearCreateCommandMother.With(
            GuidMother.Random(),
            schoolId,
            StringMother.Random("year"),
            START_DATE,
            START_DATE));

        result.Error.Should().Be(SchoolYearErrors.InvalidDateRange);
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task return_invalid_date_range_without_adding_when_the_end_date_is_before_the_start_date()
    {
        Guid schoolId = GuidMother.Random();
        this.QueryBus.ShouldFindSchool(schoolId);

        Result result = await this._useCase.ExecuteAsync(SchoolYearCreateCommandMother.With(
            GuidMother.Random(),
            schoolId,
            StringMother.Random("year"),
            END_DATE,
            START_DATE));

        result.Error.Should().Be(SchoolYearErrors.InvalidDateRange);
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task accept_an_end_date_one_day_after_the_start_date()
    {
        Guid schoolId = GuidMother.Random();
        this.QueryBus.ShouldFindSchool(schoolId);

        Result result = await this._useCase.ExecuteAsync(SchoolYearCreateCommandMother.With(
            GuidMother.Random(),
            schoolId,
            StringMother.Random("year"),
            START_DATE,
            START_DATE.AddDays(1)));

        result.IsSuccess.Should().BeTrue();
        this.Added.Should().ContainSingle();
    }
}
