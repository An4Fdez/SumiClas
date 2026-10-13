namespace SumiClas.Tests.SchoolYears.Application;

using AwesomeAssertions;
using SumiClas.SchoolYears.Application;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.SchoolYears.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class SchoolYearArchiveShould : SchoolYearsUnitTestCase
{
    private readonly SchoolYearArchive.Archive _useCase;

    public SchoolYearArchiveShould()
    {
        this._useCase = new SchoolYearArchive.Archive(this.Repository.Object, this.Clock.Object);
    }

    [Test]
    public async Task archive_the_school_year_when_it_is_active()
    {
        SchoolYear schoolYear = SchoolYearMother.Active(GuidMother.Random());
        this.ShouldReturn(schoolYear);

        Result result = await this._useCase.ExecuteAsync(
            SchoolYearArchiveCommandMother.WithSchoolYearId(schoolYear.Id));

        result.IsSuccess.Should().BeTrue();
        schoolYear.Status.Should().Be(SchoolYearStatus.Archived);
        schoolYear.IsArchived.Should().BeTrue();
    }

    [Test]
    public async Task raise_the_archived_event_with_the_school_year_id_and_the_clock_time()
    {
        SchoolYear schoolYear = SchoolYearMother.Active(GuidMother.Random());
        this.ShouldReturn(schoolYear);

        await this._useCase.ExecuteAsync(SchoolYearArchiveCommandMother.WithSchoolYearId(schoolYear.Id));

        SchoolYearArchivedDomainEvent archived = schoolYear.DomainEvents
            .Should().ContainSingle().Which.Should().BeOfType<SchoolYearArchivedDomainEvent>().Subject;
        archived.SchoolYearId.Should().Be(schoolYear.Id);
        archived.OccurredOn.Should().Be(Now);
    }

    [Test]
    public async Task return_archived_and_raise_no_second_event_when_the_school_year_is_archived_twice()
    {
        SchoolYear schoolYear = SchoolYearMother.Active(GuidMother.Random());
        this.ShouldReturn(schoolYear);
        await this._useCase.ExecuteAsync(SchoolYearArchiveCommandMother.WithSchoolYearId(schoolYear.Id));

        Result result = await this._useCase.ExecuteAsync(
            SchoolYearArchiveCommandMother.WithSchoolYearId(schoolYear.Id));

        result.Error.Should().Be(SchoolYearErrors.Archived);
        schoolYear.Status.Should().Be(SchoolYearStatus.Archived);
        schoolYear.DomainEvents.Should().ContainSingle();
    }

    [Test]
    public async Task return_archived_without_raising_events_when_the_school_year_was_stored_archived()
    {
        SchoolYear schoolYear = SchoolYearMother.Archived(GuidMother.Random());
        this.ShouldReturn(schoolYear);

        Result result = await this._useCase.ExecuteAsync(
            SchoolYearArchiveCommandMother.WithSchoolYearId(schoolYear.Id));

        result.Error.Should().Be(SchoolYearErrors.Archived);
        schoolYear.DomainEvents.Should().BeEmpty();
    }

    [Test]
    public async Task return_not_found_when_the_school_year_does_not_exist()
    {
        this.ShouldReturn(SchoolYearMother.Active(GuidMother.Random()));

        Result result = await this._useCase.ExecuteAsync(
            SchoolYearArchiveCommandMother.WithSchoolYearId(GuidMother.Random()));

        result.Error.Should().Be(SchoolYearErrors.NotFound);
    }
}
