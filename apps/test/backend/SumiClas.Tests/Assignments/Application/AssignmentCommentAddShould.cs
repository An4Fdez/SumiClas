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
using SumiClas.Tests.Assignments.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class AssignmentCommentAddShould : AssignmentsUnitTestCase
{
    private readonly AssignmentCommentAdd.Add _useCase;

    public AssignmentCommentAddShould()
    {
        this._useCase = new AssignmentCommentAdd.Add(
            this.Repository.Object,
            this.QueryBus.Object,
            this.Clock.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task add_a_trimmed_comment_after_the_existing_ones_with_the_current_user_and_the_clock_time()
    {
        Guid assignmentId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        string text = StringMother.Random("comment");
        AssignmentComment previous = AssignmentCommentMother.With(
            GuidMother.Random(),
            StringMother.Random("previous"),
            Now.AddDays(-1));
        Assignment assignment = AssignmentMother.With(id: assignmentId, comments: [previous]);
        Assignment other = AssignmentMother.With();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(assignment.ClassId, schoolYearId);
        this.ShouldAllowClassAccess(authorId, assignment.ClassId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFetchAssignments(other, assignment);
        this.ShouldCaptureAddedAssignments();
        this.AdvanceClock(TimeSpan.FromHours(3));

        Result result = await this._useCase.ExecuteAsync(
            AssignmentCommentAddCommandMother.WithText(assignmentId, "  " + text + "\n"));

        result.IsSuccess.Should().BeTrue();
        assignment.Comments.Should().HaveCount(2);
        assignment.Comments.First().Should().BeSameAs(previous);
        AssignmentComment comment = assignment.Comments.Last();
        comment.AuthorId.Should().Be(authorId);
        comment.Text.Should().Be(text);
        comment.CreatedAt.Should().Be(Now.AddHours(3));
        comment.Id.Should().NotBeEmpty();
        other.Comments.Should().BeEmpty();
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task not_require_the_author_to_be_the_class_representative()
    {
        Guid assignmentId = GuidMother.Random();
        Assignment assignment = AssignmentMother.With(id: assignmentId);
        Guid authorId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(assignment.ClassId, schoolYearId);
        this.ShouldAllowClassAccess(authorId, assignment.ClassId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFetchAssignments(assignment);

        Result result = await this._useCase.ExecuteAsync(
            AssignmentCommentAddCommandMother.WithAssignmentId(assignmentId));

        result.IsSuccess.Should().BeTrue();
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<UserIsClassRepresentative.Query>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task throw_unauthenticated_without_fetching_or_asking_when_there_is_no_current_user()
    {
        Guid assignmentId = GuidMother.Random();
        Assignment assignment = AssignmentMother.With(id: assignmentId);
        this.ShouldHaveNoCurrentUser();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AssignmentCommentAddCommandMother.WithAssignmentId(assignmentId));

        await act.Should().ThrowAsync<AuthenticationExceptions.Unauthenticated>();
        assignment.Comments.Should().BeEmpty();
        this.ShouldNotHaveFetched();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_not_found_without_asking_when_the_assignment_does_not_exist()
    {
        Guid authorId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFetchNoAssignments();

        Result result = await this._useCase.ExecuteAsync(
            AssignmentCommentAddCommandMother.WithAssignmentId(GuidMother.Random()));

        result.Error.Should().Be(AssignmentErrors.NotFound);
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task return_user_not_found_without_asking_for_the_class_when_the_author_does_not_exist()
    {
        Guid assignmentId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        Assignment assignment = AssignmentMother.With(id: assignmentId);
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldNotFindUser(authorId);
        this.ShouldFetchAssignments(assignment);

        Result result = await this._useCase.ExecuteAsync(
            AssignmentCommentAddCommandMother.WithAssignmentId(assignmentId));

        result.Error.Should().Be(UserErrors.NotFound);
        assignment.Comments.Should().BeEmpty();
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<ClassFinderById.Query>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task throw_class_not_found_without_asking_for_the_school_year_when_the_class_does_not_exist()
    {
        Guid assignmentId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        Assignment assignment = AssignmentMother.With(id: assignmentId);
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldNotFindClass();
        this.ShouldFetchAssignments(assignment);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AssignmentCommentAddCommandMother.WithAssignmentId(assignmentId));

        await act.Should().ThrowAsync<ClassExceptions.NotFound>();
        assignment.Comments.Should().BeEmpty();
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<SchoolYearEnsureIsActive.Query>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task throw_school_year_archived_without_commenting_when_the_school_year_is_archived()
    {
        Guid assignmentId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Assignment assignment = AssignmentMother.With(id: assignmentId);
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(assignment.ClassId, schoolYearId);
        this.ShouldAllowClassAccess(authorId, assignment.ClassId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, new SchoolYearExceptions.Archived());
        this.ShouldFetchAssignments(assignment);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AssignmentCommentAddCommandMother.WithAssignmentId(assignmentId));

        await act.Should().ThrowAsync<SchoolYearExceptions.Archived>();
        assignment.Comments.Should().BeEmpty();
    }

    [Test]
    public async Task throw_school_year_archived_when_the_text_is_blank_and_the_school_year_is_archived()
    {
        Guid assignmentId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Assignment assignment = AssignmentMother.With(id: assignmentId);
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(assignment.ClassId, schoolYearId);
        this.ShouldAllowClassAccess(authorId, assignment.ClassId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, new SchoolYearExceptions.Archived());
        this.ShouldFetchAssignments(assignment);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AssignmentCommentAddCommandMother.WithText(assignmentId, "  "));

        await act.Should().ThrowAsync<SchoolYearExceptions.Archived>();
    }

    [Test]
    public async Task return_comment_text_required_without_commenting_when_the_text_is_blank()
    {
        Guid assignmentId = GuidMother.Random();
        Assignment assignment = AssignmentMother.With(id: assignmentId);
        Guid authorId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(assignment.ClassId, schoolYearId);
        this.ShouldAllowClassAccess(authorId, assignment.ClassId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFetchAssignments(assignment);

        Result result = await this._useCase.ExecuteAsync(
            AssignmentCommentAddCommandMother.WithText(assignmentId, "   "));

        result.Error.Should().Be(AssignmentErrors.CommentTextRequired);
        assignment.Comments.Should().BeEmpty();
    }

    [Test]
    public async Task return_comment_author_required_when_the_author_is_empty_and_the_text_is_blank()
    {
        Guid assignmentId = GuidMother.Random();
        Assignment assignment = AssignmentMother.With(id: assignmentId);
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(Guid.Empty);
        this.ShouldFindUser(Guid.Empty);
        this.ShouldFindClassInSchoolYear(assignment.ClassId, schoolYearId);
        this.ShouldAllowClassAccess(Guid.Empty, assignment.ClassId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFetchAssignments(assignment);

        Result result = await this._useCase.ExecuteAsync(
            AssignmentCommentAddCommandMother.WithText(assignmentId, "  "));

        result.Error.Should().Be(AssignmentErrors.CommentAuthorRequired);
    }

    [Test]
    public async Task return_class_access_forbidden_without_commenting_when_the_user_does_not_belong_to_the_class()
    {
        Guid assignmentId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        Assignment assignment = AssignmentMother.With(id: assignmentId);
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(assignment.ClassId, schoolYearId);
        this.ShouldDenyClassAccess(authorId, assignment.ClassId);
        this.ShouldFetchAssignments(assignment);

        Result result = await this._useCase.ExecuteAsync(
            AssignmentCommentAddCommandMother.WithText(assignmentId, StringMother.Random("comment")));

        result.Error.Should().Be(UserErrors.ClassAccessForbidden);
        assignment.Comments.Should().BeEmpty();
    }
}
