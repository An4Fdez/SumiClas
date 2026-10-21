namespace SumiClas.Tests.Assignments.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.Assignments.Application;
using SumiClas.Assignments.Domain;
using SumiClas.Classes.Application;
using SumiClas.Classes.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Subjects.Application;
using SumiClas.Subjects.Domain;
using SumiClas.Tests.Assignments.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class AssignmentCreateShould : AssignmentsUnitTestCase
{
    private readonly AssignmentCreate.Create _useCase;

    public AssignmentCreateShould()
    {
        this._useCase = new AssignmentCreate.Create(
            this.Repository.Object,
            this.QueryBus.Object,
            this.Clock.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task add_the_official_assignment_trimmed_when_the_author_is_the_class_representative()
    {
        Guid assignmentId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        Guid subjectId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        string title = StringMother.Random("homework");
        string description = StringMother.Random("description");
        DateOnly dueDate = AssignmentMother.SomeDay;
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassAccess(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFindSubject(subjectId, classId);
        this.ShouldCaptureAddedAssignments();
        this.ShouldHaveClassRepresentative(authorId, classId);

        Result result = await this._useCase.ExecuteAsync(AssignmentCreateCommandMother.With(
            assignmentId,
            classId,
            subjectId,
            "  " + title + " ",
            "\n" + description + "  ",
            dueDate));

        result.IsSuccess.Should().BeTrue();
        Assignment added = this.Added.Single();
        added.Id.Should().Be(assignmentId);
        added.ClassId.Should().Be(classId);
        added.SubjectId.Should().Be(subjectId);
        added.Title.Should().Be(title);
        added.Description.Should().Be(description);
        added.DueDate.Should().Be(dueDate);
        added.AuthorId.Should().Be(authorId);
        added.IsProposal.Should().BeFalse();
        added.CreatedAt.Should().Be(Now);
        added.Comments.Should().BeEmpty();
    }

    [Test]
    public async Task add_a_proposal_when_the_author_is_not_the_class_representative()
    {
        Guid classId = GuidMother.Random();
        Guid subjectId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassAccess(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFindSubject(subjectId, classId);
        this.ShouldCaptureAddedAssignments();
        this.ShouldNotHaveClassRepresentative(authorId, classId);

        Result result = await this._useCase.ExecuteAsync(
            AssignmentCreateCommandMother.Valid(classId: classId, subjectId: subjectId));

        result.IsSuccess.Should().BeTrue();
        this.Added.Single().IsProposal.Should().BeTrue();
    }

    [TestCase(null)]
    [TestCase("   ")]
    public async Task add_the_assignment_without_description_when_the_description_is_blank(string? description)
    {
        Guid classId = GuidMother.Random();
        Guid subjectId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassAccess(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFindSubject(subjectId, classId);
        this.ShouldCaptureAddedAssignments();
        this.ShouldNotHaveClassRepresentative(authorId, classId);

        Result result = await this._useCase.ExecuteAsync(AssignmentCreateCommandMother.Valid(
            classId: classId,
            subjectId: subjectId,
            description: description));

        result.IsSuccess.Should().BeTrue();
        this.Added.Single().Description.Should().BeNull();
    }

    [Test]
    public async Task add_the_assignment_when_the_due_date_is_in_the_past()
    {
        Guid classId = GuidMother.Random();
        Guid subjectId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        DateOnly pastDate = AssignmentMother.SomeDay.AddDays(-30);
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassAccess(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFindSubject(subjectId, classId);
        this.ShouldCaptureAddedAssignments();
        this.ShouldNotHaveClassRepresentative(authorId, classId);

        Result result = await this._useCase.ExecuteAsync(AssignmentCreateCommandMother.Valid(
            classId: classId,
            subjectId: subjectId,
            dueDate: pastDate));

        result.IsSuccess.Should().BeTrue();
        this.Added.Single().DueDate.Should().Be(pastDate);
    }

    [Test]
    public async Task return_unauthenticated_without_asking_or_adding_when_there_is_no_current_user()
    {
        this.ShouldHaveNoCurrentUser();
        this.ShouldCaptureAddedAssignments();

        Result result = await this._useCase.ExecuteAsync(AssignmentCreateCommandMother.Valid());

        result.Error.Should().Be(AuthenticationErrors.Unauthenticated);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_author_required_without_asking_or_adding_when_the_current_user_id_is_empty()
    {
        this.ShouldResolveCurrentUser(Guid.Empty);
        this.ShouldCaptureAddedAssignments();

        Result result = await this._useCase.ExecuteAsync(AssignmentCreateCommandMother.Valid());

        result.Error.Should().Be(AssignmentErrors.AuthorRequired);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_id_required_without_asking_or_adding_when_the_id_and_the_class_are_empty()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldCaptureAddedAssignments();

        Result result = await this._useCase.ExecuteAsync(
            AssignmentCreateCommandMother.Valid(id: Guid.Empty, classId: Guid.Empty));

        result.Error.Should().Be(AssignmentErrors.IdRequired);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_class_required_without_asking_or_adding_when_the_class_and_the_subject_are_empty()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldCaptureAddedAssignments();

        Result result = await this._useCase.ExecuteAsync(
            AssignmentCreateCommandMother.Valid(classId: Guid.Empty, subjectId: Guid.Empty));

        result.Error.Should().Be(AssignmentErrors.ClassRequired);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_subject_required_without_asking_or_adding_when_the_subject_is_empty_and_title_blank()
    {
        this.ShouldResolveCurrentUser(GuidMother.Random());
        this.ShouldCaptureAddedAssignments();

        Result result = await this._useCase.ExecuteAsync(
            AssignmentCreateCommandMother.Valid(subjectId: Guid.Empty, title: "  "));

        result.Error.Should().Be(AssignmentErrors.SubjectRequired);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_title_required_without_asking_or_adding_when_the_title_is_blank_and_the_user_id_empty()
    {
        this.ShouldResolveCurrentUser(Guid.Empty);
        this.ShouldCaptureAddedAssignments();

        Result result = await this._useCase.ExecuteAsync(AssignmentCreateCommandMother.Valid(title: "  "));

        result.Error.Should().Be(AssignmentErrors.TitleRequired);
        this.Added.Should().BeEmpty();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_user_not_found_without_asking_for_the_class_when_the_author_does_not_exist()
    {
        Guid authorId = GuidMother.Random();
        Guid subjectId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldNotFindUser(authorId);
        this.ShouldCaptureAddedAssignments();

        Result result = await this._useCase.ExecuteAsync(AssignmentCreateCommandMother.Valid(subjectId: subjectId));

        result.Error.Should().Be(UserErrors.NotFound);
        this.Added.Should().BeEmpty();
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<ClassFinderById.Query>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task throw_class_not_found_without_asking_for_the_school_year_when_the_class_does_not_exist()
    {
        Guid authorId = GuidMother.Random();
        Guid subjectId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldNotFindClass();
        this.ShouldCaptureAddedAssignments();

        Func<Task> act = () => this._useCase.ExecuteAsync(AssignmentCreateCommandMother.Valid(subjectId: subjectId));

        await act.Should().ThrowAsync<ClassExceptions.NotFound>();
        this.Added.Should().BeEmpty();
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<SchoolYearEnsureIsActive.Query>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task throw_school_year_archived_without_asking_for_the_subject_when_it_is_archived()
    {
        Guid authorId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid subjectId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassAccess(authorId, classId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, new SchoolYearExceptions.Archived());
        this.ShouldCaptureAddedAssignments();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AssignmentCreateCommandMother.Valid(classId: classId, subjectId: subjectId));

        await act.Should().ThrowAsync<SchoolYearExceptions.Archived>();
        this.Added.Should().BeEmpty();
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<SubjectFinderById.Query>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task throw_school_year_not_found_without_adding_when_the_school_year_does_not_exist()
    {
        Guid authorId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassAccess(authorId, classId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, new SchoolYearExceptions.NotFound());
        this.ShouldCaptureAddedAssignments();

        Func<Task> act = () => this._useCase.ExecuteAsync(AssignmentCreateCommandMother.Valid(classId: classId));

        await act.Should().ThrowAsync<SchoolYearExceptions.NotFound>();
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task return_subject_not_found_without_asking_about_the_representative_when_it_does_not_exist()
    {
        Guid authorId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid subjectId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassAccess(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldNotFindSubject(subjectId);
        this.ShouldCaptureAddedAssignments();

        Result result = await this._useCase.ExecuteAsync(
            AssignmentCreateCommandMother.Valid(classId: classId, subjectId: subjectId));

        result.Error.Should().Be(SubjectErrors.NotFound);
        this.Added.Should().BeEmpty();
        this.ShouldNotHaveAskedWhetherTheAuthorIsRepresentative();
    }

    [Test]
    public async Task return_subject_not_in_class_without_asking_about_the_representative_for_a_foreign_subject()
    {
        Guid authorId = GuidMother.Random();
        Guid classId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Guid subjectId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldAllowClassAccess(authorId, classId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFindSubject(subjectId, GuidMother.Random());
        this.ShouldCaptureAddedAssignments();

        Result result = await this._useCase.ExecuteAsync(
            AssignmentCreateCommandMother.Valid(classId: classId, subjectId: subjectId));

        result.Error.Should().Be(AssignmentErrors.SubjectNotInClass);
        this.Added.Should().BeEmpty();
        this.ShouldNotHaveAskedWhetherTheAuthorIsRepresentative();
    }

    private void ShouldNotHaveAskedWhetherTheAuthorIsRepresentative()
        => this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<UserIsClassRepresentative.Query>(), It.IsAny<CancellationToken>()),
            Times.Never);

    [Test]
    public async Task return_class_access_forbidden_without_adding_when_the_author_does_not_belong_to_the_class()
    {
        this.ShouldCaptureAddedAssignments();
        Guid classId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(classId, schoolYearId);
        this.ShouldDenyClassAccess(authorId, classId);

        Result result = await this._useCase.ExecuteAsync(AssignmentCreateCommandMother.Valid(classId: classId));

        result.Error.Should().Be(UserErrors.ClassAccessForbidden);
        this.Added.Should().BeEmpty();
    }
}
