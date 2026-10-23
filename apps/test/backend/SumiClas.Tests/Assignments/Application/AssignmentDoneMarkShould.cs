namespace SumiClas.Tests.Assignments.Application;

using AwesomeAssertions;
using SumiClas.Assignments.Application;
using SumiClas.Assignments.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Assignments.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class AssignmentDoneMarkShould : AssignmentsUnitTestCase
{
    private readonly AssignmentDoneMark.Mark _useCase;

    public AssignmentDoneMarkShould()
    {
        this._useCase = new AssignmentDoneMark.Mark(
            this.Repository.Object,
            this.QueryBus.Object,
            this.Clock.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task add_a_completion_for_the_current_user_with_the_clock_time_only_to_the_requested_assignment()
    {
        Guid assignmentId = GuidMother.Random();
        Guid userId = GuidMother.Random();
        Assignment assignment = AssignmentMother.With(id: assignmentId);
        Assignment other = AssignmentMother.With();
        this.ShouldFetchAssignments(other, assignment);
        this.ShouldResolveCurrentUser(userId);
        this.ShouldFindUser(userId);
        this.ShouldAllowClassAccess(userId, assignment.ClassId);

        await this._useCase.ExecuteAsync(AssignmentDoneMarkCommandMother.WithAssignmentId(assignmentId));

        AssignmentCompletion completion = assignment.Completions.Should().ContainSingle().Subject;
        completion.UserId.Should().Be(userId);
        completion.CompletedAt.Should().Be(Now);
        other.Completions.Should().BeEmpty();
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task keep_the_original_completion_when_the_same_user_marks_it_again()
    {
        Guid assignmentId = GuidMother.Random();
        Guid userId = GuidMother.Random();
        Assignment assignment = AssignmentMother.With(id: assignmentId);
        this.ShouldFetchAssignments(assignment);
        this.ShouldResolveCurrentUser(userId);
        this.ShouldFindUser(userId);
        this.ShouldAllowClassAccess(userId, assignment.ClassId);
        await this._useCase.ExecuteAsync(AssignmentDoneMarkCommandMother.WithAssignmentId(assignmentId));
        this.AdvanceClock(TimeSpan.FromHours(2));

        await this._useCase.ExecuteAsync(AssignmentDoneMarkCommandMother.WithAssignmentId(assignmentId));

        AssignmentCompletion completion = assignment.Completions.Should().ContainSingle().Subject;
        completion.UserId.Should().Be(userId);
        completion.CompletedAt.Should().Be(Now);
    }

    [Test]
    public async Task keep_one_completion_per_user_when_several_users_mark_it()
    {
        Guid assignmentId = GuidMother.Random();
        Guid firstUserId = GuidMother.Random();
        Guid secondUserId = GuidMother.Random();
        Assignment assignment = AssignmentMother.With(id: assignmentId);
        this.ShouldFetchAssignments(assignment);
        this.ShouldResolveCurrentUser(firstUserId);
        this.ShouldFindUser(firstUserId);
        this.ShouldAllowClassAccess(firstUserId, assignment.ClassId);
        await this._useCase.ExecuteAsync(AssignmentDoneMarkCommandMother.WithAssignmentId(assignmentId));
        this.AdvanceClock(TimeSpan.FromHours(2));
        this.ShouldResolveCurrentUser(secondUserId);
        this.ShouldFindUser(secondUserId);
        this.ShouldAllowClassAccess(secondUserId, assignment.ClassId);

        await this._useCase.ExecuteAsync(AssignmentDoneMarkCommandMother.WithAssignmentId(assignmentId));

        assignment.Completions.Select(completion => completion.UserId)
            .Should().BeEquivalentTo(new[] { firstUserId, secondUserId });
        assignment.Completions.Single(completion => completion.UserId == secondUserId).CompletedAt
            .Should().Be(Now.AddHours(2));
        assignment.Completions.Single(completion => completion.UserId == firstUserId).CompletedAt
            .Should().Be(Now);
    }

    [Test]
    public async Task throw_unauthenticated_without_reading_the_assignment_when_there_is_no_current_user()
    {
        Guid assignmentId = GuidMother.Random();
        Assignment assignment = AssignmentMother.With(id: assignmentId);
        this.ShouldFetchAssignments(assignment);
        this.ShouldHaveNoCurrentUser();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AssignmentDoneMarkCommandMother.WithAssignmentId(assignmentId));

        await act.Should().ThrowAsync<AuthenticationExceptions.Unauthenticated>();
        assignment.Completions.Should().BeEmpty();
        this.ShouldNotHaveFetched();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_not_found_without_asking_for_the_user_when_the_assignment_does_not_exist()
    {
        Guid userId = GuidMother.Random();
        this.ShouldFetchAssignments(AssignmentMother.With());
        this.ShouldResolveCurrentUser(userId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AssignmentDoneMarkCommandMother.WithAssignmentId(GuidMother.Random()));

        await act.Should().ThrowAsync<AssignmentExceptions.NotFound>();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_user_not_found_without_adding_a_completion_when_the_user_does_not_exist()
    {
        Guid assignmentId = GuidMother.Random();
        Guid userId = GuidMother.Random();
        Assignment assignment = AssignmentMother.With(id: assignmentId);
        this.ShouldFetchAssignments(assignment);
        this.ShouldResolveCurrentUser(userId);
        this.ShouldNotFindUser(userId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AssignmentDoneMarkCommandMother.WithAssignmentId(assignmentId));

        await act.Should().ThrowAsync<UserExceptions.NotFound>();
        assignment.Completions.Should().BeEmpty();
    }

    [Test]
    public async Task throw_class_access_forbidden_without_adding_a_completion_when_the_user_is_not_in_the_class()
    {
        Guid assignmentId = GuidMother.Random();
        Guid userId = GuidMother.Random();
        Assignment assignment = AssignmentMother.With(id: assignmentId);
        this.ShouldFetchAssignments(assignment);
        this.ShouldResolveCurrentUser(userId);
        this.ShouldFindUser(userId);
        this.ShouldDenyClassAccess(userId, assignment.ClassId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AssignmentDoneMarkCommandMother.WithAssignmentId(assignmentId));

        await act.Should().ThrowAsync<UserExceptions.ClassAccessForbidden>();
        assignment.Completions.Should().BeEmpty();
    }

    [Test]
    public async Task throw_completion_user_required_without_adding_a_completion_when_the_user_id_is_empty()
    {
        Guid assignmentId = GuidMother.Random();
        Assignment assignment = AssignmentMother.With(id: assignmentId);
        this.ShouldFetchAssignments(assignment);
        this.ShouldResolveCurrentUser(Guid.Empty);
        this.ShouldFindUser(Guid.Empty);
        this.ShouldAllowClassAccess(Guid.Empty, assignment.ClassId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AssignmentDoneMarkCommandMother.WithAssignmentId(assignmentId));

        await act.Should().ThrowAsync<AssignmentExceptions.CompletionUserRequired>();
        assignment.Completions.Should().BeEmpty();
    }
}
