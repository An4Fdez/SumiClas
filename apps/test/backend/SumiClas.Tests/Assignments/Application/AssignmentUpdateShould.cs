namespace SumiClas.Tests.Assignments.Application;

using AwesomeAssertions;
using SumiClas.Assignments.Application;
using SumiClas.Assignments.Domain;
using SumiClas.Tests.Assignments.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class AssignmentUpdateShould : AssignmentsUnitTestCase
{
    private readonly AssignmentUpdate.Update _useCase;

    public AssignmentUpdateShould()
    {
        this._useCase = new AssignmentUpdate.Update(
            this.Repository.Object,
            this.QueryBus.Object,
            this.Clock.Object,
            this.UserProvider.Object);
    }

    [Test]
    public async Task throw_title_required_without_asking_the_bus_when_the_new_title_is_blank()
    {
        Guid authorId = GuidMother.Random();
        Assignment assignment = AssignmentMother.With(authorId: authorId);
        string title = assignment.Title;
        this.ShouldFetchAssignments(assignment);
        this.ShouldResolveCurrentUser(authorId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            AssignmentUpdateCommandMother.Valid(assignment.Id, title: " "));

        await act.Should().ThrowAsync<AssignmentExceptions.TitleRequired>();
        assignment.Title.Should().Be(title);
        this.QueryBus.VerifyNoOtherCalls();
    }
}
