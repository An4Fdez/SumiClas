namespace SumiClas.Tests.SchoolYears.Application;

using AwesomeAssertions;
using SumiClas.SchoolYears.Application;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.SchoolYears.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Users.Domain;

[TestFixture]
public class SchoolYearArchiveShould : SchoolYearsUnitTestCase
{
    private readonly SchoolYearArchive.Archive _useCase;

    public SchoolYearArchiveShould()
    {
        this._useCase = new SchoolYearArchive.Archive(
            this.Repository.Object,
            this.QueryBus.Object,
            this.Clock.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task archive_the_school_year_when_the_user_is_a_delegate_of_one_of_its_classes()
    {
        SchoolYear schoolYear = SchoolYearMother.Active(GuidMother.Random());
        Guid otherClassId = GuidMother.Random();
        Guid delegateClassId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldFetchSchoolYears(schoolYear);
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldFindClassesOfSchoolYear(schoolYear.Id, otherClassId, delegateClassId);
        this.ShouldAllowAnyClassRepresentative(currentUserId, otherClassId, delegateClassId);

        await this._useCase.ExecuteAsync(
            SchoolYearArchiveCommandMother.WithSchoolYearId(schoolYear.Id));

        schoolYear.Status.Should().Be(SchoolYearStatus.Archived);
        schoolYear.IsArchived.Should().BeTrue();
    }

    [Test]
    public async Task raise_the_archived_event_with_the_school_year_id_and_the_clock_time()
    {
        SchoolYear schoolYear = SchoolYearMother.Active(GuidMother.Random());
        Guid classId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldFetchSchoolYears(schoolYear);
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldFindClassesOfSchoolYear(schoolYear.Id, classId);
        this.ShouldAllowAnyClassRepresentative(currentUserId, classId);

        await this._useCase.ExecuteAsync(SchoolYearArchiveCommandMother.WithSchoolYearId(schoolYear.Id));

        SchoolYearArchivedDomainEvent archived = schoolYear.DomainEvents
            .Should().ContainSingle().Which.Should().BeOfType<SchoolYearArchivedDomainEvent>().Subject;
        archived.SchoolYearId.Should().Be(schoolYear.Id);
        archived.OccurredOn.Should().Be(Now);
    }

    [Test]
    public async Task throw_archived_and_raise_no_second_event_when_the_school_year_is_archived_twice()
    {
        SchoolYear schoolYear = SchoolYearMother.Active(GuidMother.Random());
        Guid classId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldFetchSchoolYears(schoolYear);
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldFindClassesOfSchoolYear(schoolYear.Id, classId);
        this.ShouldAllowAnyClassRepresentative(currentUserId, classId);
        await this._useCase.ExecuteAsync(SchoolYearArchiveCommandMother.WithSchoolYearId(schoolYear.Id));

        Func<Task> act = () => this._useCase.ExecuteAsync(
            SchoolYearArchiveCommandMother.WithSchoolYearId(schoolYear.Id));

        await act.Should().ThrowAsync<SchoolYearExceptions.Archived>();
        schoolYear.Status.Should().Be(SchoolYearStatus.Archived);
        schoolYear.DomainEvents.Should().ContainSingle();
    }

    [Test]
    public async Task throw_class_representative_forbidden_and_keep_it_active_when_the_user_is_not_a_delegate()
    {
        SchoolYear schoolYear = SchoolYearMother.Active(GuidMother.Random());
        Guid firstClassId = GuidMother.Random();
        Guid secondClassId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        this.ShouldFetchSchoolYears(schoolYear);
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldFindClassesOfSchoolYear(schoolYear.Id, firstClassId, secondClassId);
        this.ShouldDenyAnyClassRepresentative(currentUserId, firstClassId, secondClassId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            SchoolYearArchiveCommandMother.WithSchoolYearId(schoolYear.Id));

        await act.Should().ThrowAsync<UserExceptions.ClassRepresentativeForbidden>();
        schoolYear.Status.Should().Be(SchoolYearStatus.Active);
        schoolYear.DomainEvents.Should().BeEmpty();
    }

    [Test]
    public async Task throw_class_representative_forbidden_and_keep_it_active_when_the_school_year_has_no_classes()
    {
        SchoolYear schoolYear = SchoolYearMother.Active(GuidMother.Random());
        this.ShouldFetchSchoolYears(schoolYear);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldFindClassesOfSchoolYear(schoolYear.Id);
        this.ShouldDenyAnyClassRepresentative(currentUserId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            SchoolYearArchiveCommandMother.WithSchoolYearId(schoolYear.Id));

        await act.Should().ThrowAsync<UserExceptions.ClassRepresentativeForbidden>();
        schoolYear.Status.Should().Be(SchoolYearStatus.Active);
    }

    [Test]
    public async Task throw_unauthenticated_and_keep_it_active_when_there_is_no_current_user()
    {
        SchoolYear schoolYear = SchoolYearMother.Active(GuidMother.Random());
        this.ShouldFetchSchoolYears(schoolYear);
        this.ShouldHaveNoCurrentUser();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            SchoolYearArchiveCommandMother.WithSchoolYearId(schoolYear.Id));

        await act.Should().ThrowAsync<AuthenticationExceptions.Unauthenticated>();
        schoolYear.Status.Should().Be(SchoolYearStatus.Active);
    }

    [Test]
    public async Task throw_not_found_when_the_school_year_does_not_exist()
    {
        this.ShouldFetchSchoolYears(SchoolYearMother.Active(GuidMother.Random()));

        Func<Task> act = () => this._useCase.ExecuteAsync(
            SchoolYearArchiveCommandMother.WithSchoolYearId(GuidMother.Random()));

        await act.Should().ThrowAsync<SchoolYearExceptions.NotFound>();
    }
}
