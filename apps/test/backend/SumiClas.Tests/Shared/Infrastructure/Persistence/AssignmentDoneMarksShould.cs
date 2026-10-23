namespace SumiClas.Tests.Shared.Infrastructure.Persistence;

using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using SumiClas.Assignments.Domain;
using SumiClas.Shared.Infrastructure.Persistence;
using SumiClas.Tests.Assignments.Domain;

/// <summary>
/// The done marks of a homework over a real PostgreSQL: marking and unmarking an assignment that is already stored must
/// insert and delete rows of the owned collection, and each user's mark must stay independent of the others.
/// </summary>
[TestFixture]
public class AssignmentDoneMarksShould : UnitTestCase
{
    [Test]
    public async Task keep_a_mark_added_to_a_stored_assignment()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        Assignment stored = AssignmentMother.With();
        Guid userId = Guid.NewGuid();
        await SaveAsync(database, stored);
        await ChangeAsync(database, stored.Id, assignment => assignment.MarkAsDoneBy(userId, this.Clock.Object));

        Assignment reloaded = await LoadAsync(database, stored.Id);

        reloaded.Completions.Should().ContainSingle().Which.UserId.Should().Be(userId);
        reloaded.Completions.Single().CompletedAt.Should().Be(Now);
    }

    [Test]
    public async Task delete_only_the_mark_of_the_user_that_unmarks_a_stored_assignment()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        Guid unmarking = Guid.NewGuid();
        Guid classmate = Guid.NewGuid();
        Assignment stored = AssignmentMother.DoneBy(Guid.NewGuid(), unmarking);
        stored.MarkAsDoneBy(classmate, this.Clock.Object);
        await SaveAsync(database, stored);
        await ChangeAsync(database, stored.Id, assignment => assignment.UnmarkAsDoneBy(unmarking));

        Assignment reloaded = await LoadAsync(database, stored.Id);

        reloaded.IsDoneBy(unmarking).Should().BeFalse();
        reloaded.IsDoneBy(classmate).Should().BeTrue();
    }

    [Test]
    public async Task mark_again_after_unmarking()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        Guid userId = Guid.NewGuid();
        Assignment stored = AssignmentMother.DoneBy(Guid.NewGuid(), userId);
        await SaveAsync(database, stored);
        await ChangeAsync(database, stored.Id, assignment => assignment.UnmarkAsDoneBy(userId));
        await ChangeAsync(database, stored.Id, assignment => assignment.MarkAsDoneBy(userId, this.Clock.Object));

        Assignment reloaded = await LoadAsync(database, stored.Id);

        reloaded.Completions.Should().ContainSingle().Which.UserId.Should().Be(userId);
    }

    private static async Task SaveAsync(TestDatabase database, Assignment assignment)
    {
        await using SumiClasDbContext context = database.NewContext();
        context.Assignments.Add(assignment);
        await context.SaveChangesAsync();
    }

    private static async Task ChangeAsync(TestDatabase database, Guid assignmentId, Action<Assignment> change)
    {
        await using SumiClasDbContext context = database.NewContext();
        Assignment assignment = await context.Assignments.SingleAsync(candidate => candidate.Id == assignmentId);
        change(assignment);
        await context.SaveChangesAsync();
    }

    private static async Task<Assignment> LoadAsync(TestDatabase database, Guid assignmentId)
    {
        await using SumiClasDbContext context = database.NewContext();
        return await context.Assignments.SingleAsync(candidate => candidate.Id == assignmentId);
    }
}
