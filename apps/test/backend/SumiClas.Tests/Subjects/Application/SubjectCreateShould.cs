namespace SumiClas.Tests.Subjects.Application;

using AwesomeAssertions;
using SumiClas.Classes.Domain;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Subjects.Application;
using SumiClas.Subjects.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Users.Domain;

[TestFixture]
public class SubjectCreateShould : SubjectsUnitTestCase
{
    private readonly SubjectCreate.Create _useCase;

    public SubjectCreateShould()
    {
        this._useCase = new SubjectCreate.Create(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task add_the_subject_with_its_data_and_the_trimmed_label_when_the_school_year_is_active()
    {
        Guid subjectId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        string label = StringMother.Random("subject");
        Guid schoolYearId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassRepresentative(currentUserId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldCaptureAddedSubjects();

        Result result = await this._useCase.ExecuteAsync(
            SubjectCreateCommandMother.With(subjectId, classId, "  " + label + " "));

        result.IsSuccess.Should().BeTrue();
        Subject added = this.Added.Single();
        added.Id.Should().Be(subjectId);
        added.ClassId.Should().Be(classId);
        added.Label.Should().Be(label);
        added.DomainEvents.Should().BeEmpty();
    }

    [Test]
    public async Task throw_class_not_found_without_adding_when_the_class_does_not_exist()
    {
        this.ShouldCaptureAddedSubjects();
        this.ShouldNotFindClass();

        Func<Task> act = () => this._useCase.ExecuteAsync(SubjectCreateCommandMother.With(
            GuidMother.Random(),
            GuidMother.Random(),
            StringMother.Random("subject")));

        await act.Should().ThrowAsync<ClassExceptions.NotFound>();
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task throw_school_year_archived_without_adding_when_the_school_year_is_archived()
    {
        this.ShouldCaptureAddedSubjects();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassRepresentative(currentUserId, classId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, new SchoolYearExceptions.Archived());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            SubjectCreateCommandMother.With(GuidMother.Random(), classId, StringMother.Random("subject")));

        await act.Should().ThrowAsync<SchoolYearExceptions.Archived>();
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task throw_school_year_not_found_without_adding_when_the_school_year_does_not_exist()
    {
        this.ShouldCaptureAddedSubjects();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        Guid currentUserId = GuidMother.Random();
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldAllowClassRepresentative(currentUserId, classId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, new SchoolYearExceptions.NotFound());

        Func<Task> act = () => this._useCase.ExecuteAsync(
            SubjectCreateCommandMother.With(GuidMother.Random(), classId, StringMother.Random("subject")));

        await act.Should().ThrowAsync<SchoolYearExceptions.NotFound>();
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task return_label_required_without_asking_or_adding_when_the_label_is_blank()
    {
        this.ShouldCaptureAddedSubjects();
        Result result = await this._useCase.ExecuteAsync(
            SubjectCreateCommandMother.With(GuidMother.Random(), GuidMother.Random(), "   "));

        result.Error.Should().Be(SubjectErrors.LabelRequired);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_id_required_without_asking_or_adding_when_every_field_is_invalid()
    {
        this.ShouldCaptureAddedSubjects();
        Result result = await this._useCase.ExecuteAsync(
            SubjectCreateCommandMother.With(Guid.Empty, Guid.Empty, " "));

        result.Error.Should().Be(SubjectErrors.IdRequired);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_class_required_without_asking_or_adding_when_the_class_id_and_the_label_are_invalid()
    {
        this.ShouldCaptureAddedSubjects();
        Result result = await this._useCase.ExecuteAsync(
            SubjectCreateCommandMother.With(GuidMother.Random(), Guid.Empty, " "));

        result.Error.Should().Be(SubjectErrors.ClassRequired);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_class_representative_forbidden_without_adding_when_the_user_is_not_a_delegate()
    {
        this.ShouldCaptureAddedSubjects();
        Guid classId = GuidMother.Random();
        Guid currentUserId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldResolveCurrentUser(currentUserId);
        this.ShouldDenyClassRepresentative(currentUserId, classId);

        Result result = await this._useCase.ExecuteAsync(
            SubjectCreateCommandMother.With(GuidMother.Random(), classId, StringMother.Random("subject")));

        result.Error.Should().Be(UserErrors.ClassRepresentativeForbidden);
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task return_unauthenticated_without_adding_when_there_is_no_current_user()
    {
        this.ShouldCaptureAddedSubjects();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldHaveNoCurrentUser();

        Result result = await this._useCase.ExecuteAsync(
            SubjectCreateCommandMother.With(GuidMother.Random(), classId, StringMother.Random("subject")));

        result.Error.Should().Be(AuthenticationErrors.Unauthenticated);
        this.Added.Should().BeEmpty();
    }
}
