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
public class AssignmentDoneUnmarkShould : AssignmentsUnitTestCase
{
    private readonly AssignmentDoneUnmark.Unmark _useCase;

    public AssignmentDoneUnmarkShould()
    {
        this._useCase = new AssignmentDoneUnmark.Unmark(
            this.Repository.Object,
            this.QueryBus.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task remove_only_the_completion_of_the_current_user_from_the_requested_assignment()
    {
        Guid assignmentId = GuidMother.Random();
        Guid userId = GuidMother.Random();
        Guid classmateId = GuidMother.Random();
        Assignment assignment = AssignmentMother.DoneBy(assignmentId, userId);
        assignment.MarkAsDoneBy(classmateId, this.Clock.Object);
        Assignment other = AssignmentMother.DoneBy(GuidMother.Random(), userId);
        this.ShouldFetchAssignments(other, assignment);
        this.ShouldResolveCurrentUser(userId);
        this.ShouldFindUser(userId);
        this.ShouldAllowClassAccess(userId, assignment.ClassId);

        await this._useCase.ExecuteAsync(AssignmentDoneUnmarkCommandMother.WithAssignmentId(assignmentId));

        assignment.Completions.Should().ContainSingle().Which.UserId.Should().Be(classmateId);
        other.Completions.Should().ContainSingle().Which.UserId.Should().Be(userId);
        this.Added.Should().BeEmpty();
    }

    [Test]
    public async Task do_nothing_when_the_user_had_not_marked_the_assignment()
    {
        Guid assignmentId = GuidMother.Random();
        Guid userId = GuidMother.Random();
        Guid classmateId = GuidMother.Random();
        Assignment assignment = AssignmentMother.DoneBy(assignmentId, classmateId);
        this.ShouldFetchAssignments(assignment);
        this.ShouldResolveCurrentUser(userId);
        this.ShouldFindUser(userId);
        this.ShouldAllowClassAccess(userId, assignment.ClassId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AssignmentDoneUnmarkCommandMother.WithAssignmentId(assignmentId));

        await act.Should().NotThrowAsync();
        assignment.Completions.Should().ContainSingle().Which.UserId.Should().Be(classmateId);
    }

    [Test]
    public async Task throw_unauthenticated_without_reading_the_assignment_when_there_is_no_current_user()
    {
        Guid assignmentId = GuidMother.Random();
        Assignment assignment = AssignmentMother.DoneBy(assignmentId, GuidMother.Random());
        this.ShouldFetchAssignments(assignment);
        this.ShouldHaveNoCurrentUser();

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AssignmentDoneUnmarkCommandMother.WithAssignmentId(assignmentId));

        await act.Should().ThrowAsync<AuthenticationExceptions.Unauthenticated>();
        assignment.Completions.Should().ContainSingle();
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
            AssignmentDoneUnmarkCommandMother.WithAssignmentId(GuidMother.Random()));

        await act.Should().ThrowAsync<AssignmentExceptions.NotFound>();
        this.QueryBus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task throw_user_not_found_without_removing_the_completion_when_the_user_does_not_exist()
    {
        Guid assignmentId = GuidMother.Random();
        Guid userId = GuidMother.Random();
        Assignment assignment = AssignmentMother.DoneBy(assignmentId, userId);
        this.ShouldFetchAssignments(assignment);
        this.ShouldResolveCurrentUser(userId);
        this.ShouldNotFindUser(userId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AssignmentDoneUnmarkCommandMother.WithAssignmentId(assignmentId));

        await act.Should().ThrowAsync<UserExceptions.NotFound>();
        assignment.Completions.Should().ContainSingle();
    }

    [Test]
    public async Task throw_class_access_forbidden_without_removing_the_completion_when_the_user_left_the_class()
    {
        Guid assignmentId = GuidMother.Random();
        Guid userId = GuidMother.Random();
        Assignment assignment = AssignmentMother.DoneBy(assignmentId, userId);
        this.ShouldFetchAssignments(assignment);
        this.ShouldResolveCurrentUser(userId);
        this.ShouldFindUser(userId);
        this.ShouldDenyClassAccess(userId, assignment.ClassId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AssignmentDoneUnmarkCommandMother.WithAssignmentId(assignmentId));

        await act.Should().ThrowAsync<UserExceptions.ClassAccessForbidden>();
        assignment.Completions.Should().ContainSingle();
    }
}
