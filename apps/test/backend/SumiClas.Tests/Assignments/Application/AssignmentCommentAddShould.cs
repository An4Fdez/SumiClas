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
    public async Task add_a_trimmed_comment_with_the_current_user_and_the_clock_time()
    {
        Guid assignmentId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        string text = StringMother.Random("comment");
        Assignment assignment = AssignmentMother.With(id: assignmentId);
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(assignment.ClassId, schoolYearId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFetchAssignments(assignment);
        this.ShouldCaptureAddedAssignments();

        Result result = await this._useCase.ExecuteAsync(
            AssignmentCommentAddCommandMother.WithText(assignmentId, "  " + text + "\n"));

        result.IsSuccess.Should().BeTrue();
        AssignmentComment comment = assignment.Comments.Should().ContainSingle().Subject;
        comment.AuthorId.Should().Be(authorId);
        comment.Text.Should().Be(text);
        comment.CreatedAt.Should().Be(Now);
        comment.Id.Should().NotBeEmpty();
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task keep_the_existing_comments_and_append_the_new_one_with_its_own_time()
    {
        Guid assignmentId = GuidMother.Random();
        Guid previousAuthorId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        AssignmentComment previous = AssignmentCommentMother.With(
            previousAuthorId,
            StringMother.Random("previous"),
            Now.AddDays(-1));
        Assignment assignment = AssignmentMother.With(id: assignmentId, comments: [previous]);
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(assignment.ClassId, schoolYearId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFetchAssignments(assignment);
        this.AdvanceClock(TimeSpan.FromHours(3));

        Result result = await this._useCase.ExecuteAsync(
            AssignmentCommentAddCommandMother.WithAssignmentId(assignmentId));

        result.IsSuccess.Should().BeTrue();
        assignment.Comments.Should().HaveCount(2);
        assignment.Comments.First().Should().BeSameAs(previous);
        assignment.Comments.Last().AuthorId.Should().Be(authorId);
        assignment.Comments.Last().CreatedAt.Should().Be(Now.AddHours(3));
    }

    [Test]
    public async Task comment_only_the_requested_assignment()
    {
        Guid assignmentId = GuidMother.Random();
        Assignment requested = AssignmentMother.With(id: assignmentId);
        Assignment other = AssignmentMother.With();
        Guid authorId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(requested.ClassId, schoolYearId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFetchAssignments(other, requested);

        await this._useCase.ExecuteAsync(AssignmentCommentAddCommandMother.WithAssignmentId(assignmentId));

        requested.Comments.Should().HaveCount(1);
        other.Comments.Should().BeEmpty();
    }

    [Test]
    public async Task not_check_that_the_author_belongs_to_the_class_nor_is_the_representative()
    {
        Guid assignmentId = GuidMother.Random();
        Assignment assignment = AssignmentMother.With(id: assignmentId);
        Guid authorId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(assignment.ClassId, schoolYearId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFetchAssignments(assignment);

        Result result = await this._useCase.ExecuteAsync(
            AssignmentCommentAddCommandMother.WithAssignmentId(assignmentId));

        result.IsSuccess.Should().BeTrue();
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<UserIsClassRepresentative.Query>(), It.IsAny<CancellationToken>()),
            Times.Never);
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<UserClassMemberFinderByIds.Query>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task return_unauthenticated_without_fetching_or_asking_when_there_is_no_current_user()
    {
        Guid assignmentId = GuidMother.Random();
        Assignment assignment = AssignmentMother.With(id: assignmentId);
        this.ShouldHaveNoCurrentUser();

        Result result = await this._useCase.ExecuteAsync(
            AssignmentCommentAddCommandMother.WithAssignmentId(assignmentId));

        result.Error.Should().Be(AuthenticationErrors.Unauthenticated);
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
    public async Task return_class_not_found_without_asking_for_the_school_year_when_the_class_does_not_exist()
    {
        Guid assignmentId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        Assignment assignment = AssignmentMother.With(id: assignmentId);
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldNotFindClass();
        this.ShouldFetchAssignments(assignment);

        Result result = await this._useCase.ExecuteAsync(
            AssignmentCommentAddCommandMother.WithAssignmentId(assignmentId));

        result.Error.Should().Be(ClassErrors.NotFound);
        assignment.Comments.Should().BeEmpty();
        this.QueryBus.Verify(
            bus => bus.Send(It.IsAny<SchoolYearEnsureIsActive.Query>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task return_school_year_archived_without_commenting_when_the_school_year_is_archived()
    {
        Guid assignmentId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Assignment assignment = AssignmentMother.With(id: assignmentId);
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(assignment.ClassId, schoolYearId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, SchoolYearErrors.Archived);
        this.ShouldFetchAssignments(assignment);

        Result result = await this._useCase.ExecuteAsync(
            AssignmentCommentAddCommandMother.WithAssignmentId(assignmentId));

        result.Error.Should().Be(SchoolYearErrors.Archived);
        assignment.Comments.Should().BeEmpty();
    }

    [Test]
    public async Task return_school_year_archived_when_the_text_is_blank_and_the_school_year_is_archived()
    {
        Guid assignmentId = GuidMother.Random();
        Guid authorId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        Assignment assignment = AssignmentMother.With(id: assignmentId);
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(assignment.ClassId, schoolYearId);
        this.ShouldFailEnsuringSchoolYearIsActive(schoolYearId, SchoolYearErrors.Archived);
        this.ShouldFetchAssignments(assignment);

        Result result = await this._useCase.ExecuteAsync(
            AssignmentCommentAddCommandMother.WithText(assignmentId, "  "));

        result.Error.Should().Be(SchoolYearErrors.Archived);
    }

    [TestCase("")]
    [TestCase("   ")]
    public async Task return_comment_text_required_without_commenting_when_the_text_is_blank(string text)
    {
        Guid assignmentId = GuidMother.Random();
        Assignment assignment = AssignmentMother.With(id: assignmentId);
        Guid authorId = GuidMother.Random();
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(authorId);
        this.ShouldFindUser(authorId);
        this.ShouldFindClassInSchoolYear(assignment.ClassId, schoolYearId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFetchAssignments(assignment);

        Result result = await this._useCase.ExecuteAsync(
            AssignmentCommentAddCommandMother.WithText(assignmentId, text));

        result.Error.Should().Be(AssignmentErrors.CommentTextRequired);
        assignment.Comments.Should().BeEmpty();
    }

    [Test]
    public async Task return_comment_author_required_without_commenting_when_the_current_user_id_is_empty()
    {
        Guid assignmentId = GuidMother.Random();
        Assignment assignment = AssignmentMother.With(id: assignmentId);
        Guid schoolYearId = GuidMother.Random();
        this.ShouldResolveCurrentUser(Guid.Empty);
        this.ShouldFindUser(Guid.Empty);
        this.ShouldFindClassInSchoolYear(assignment.ClassId, schoolYearId);
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFetchAssignments(assignment);

        Result result = await this._useCase.ExecuteAsync(
            AssignmentCommentAddCommandMother.WithAssignmentId(assignmentId));

        result.Error.Should().Be(AssignmentErrors.CommentAuthorRequired);
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
        this.ShouldEnsureSchoolYearIsActive(schoolYearId);
        this.ShouldFetchAssignments(assignment);

        Result result = await this._useCase.ExecuteAsync(
            AssignmentCommentAddCommandMother.WithText(assignmentId, "  "));

        result.Error.Should().Be(AssignmentErrors.CommentAuthorRequired);
    }
}
