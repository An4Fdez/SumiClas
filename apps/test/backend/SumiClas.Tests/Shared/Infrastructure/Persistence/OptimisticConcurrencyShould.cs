namespace SumiClas.Tests.Shared.Infrastructure.Persistence;

using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using SumiClas.Sessions.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Infrastructure.Persistence;
using SumiClas.Tests.Sessions.Domain;

/// <summary>
/// The optimistic concurrency of a session over a real PostgreSQL: two requests that rotate the same refresh token at
/// once must not both win, because that would leave two active successors of one token.
/// </summary>
[TestFixture]
public class OptimisticConcurrencyShould : UnitTestCase
{
    private const string CURRENT_HASH = "hash-current";

    [Test]
    public async Task raise_a_concurrency_conflict_when_two_contexts_rotate_the_same_session()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        Guid sessionId = await SaveSessionAsync(database);
        await using SumiClasDbContext first = database.NewContext();
        await using SumiClasDbContext second = database.NewContext();
        Session rotatedByFirst = await LoadAsync(first, sessionId);
        Session rotatedBySecond = await LoadAsync(second, sessionId);
        rotatedByFirst.Rotate(CURRENT_HASH, "hash-first", this.Clock.Object);
        rotatedBySecond.Rotate(CURRENT_HASH, "hash-second", this.Clock.Object);
        await first.SaveChangesAsync();

        Func<Task> act = () => second.SaveChangesAsync();

        await act.Should().ThrowAsync<ConcurrencyConflictException>();
    }

    [Test]
    public async Task keep_the_successor_of_the_first_rotation_after_a_conflict()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        Guid sessionId = await SaveSessionAsync(database);
        await using SumiClasDbContext first = database.NewContext();
        await using SumiClasDbContext second = database.NewContext();
        (await LoadAsync(first, sessionId)).Rotate(CURRENT_HASH, "hash-first", this.Clock.Object);
        (await LoadAsync(second, sessionId)).Rotate(CURRENT_HASH, "hash-second", this.Clock.Object);
        await first.SaveChangesAsync();
        try
        {
            await second.SaveChangesAsync();
        }
        catch (ConcurrencyConflictException)
        {
            // expected: what matters is what the database keeps
        }

        await using SumiClasDbContext reader = database.NewContext();
        Session stored = await LoadAsync(reader, sessionId);
        stored.RefreshTokens.Select(token => token.TokenHash).Should().Contain("hash-first")
            .And.NotContain("hash-second");
    }

    [Test]
    public async Task save_two_rotations_made_one_after_the_other()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        Guid sessionId = await SaveSessionAsync(database);
        await using SumiClasDbContext first = database.NewContext();
        (await LoadAsync(first, sessionId)).Rotate(CURRENT_HASH, "hash-first", this.Clock.Object);
        await first.SaveChangesAsync();
        this.AdvanceClock(TimeSpan.FromMinutes(1));
        await using SumiClasDbContext second = database.NewContext();
        (await LoadAsync(second, sessionId)).Rotate("hash-first", "hash-second", this.Clock.Object);

        Func<Task> act = () => second.SaveChangesAsync();

        await act.Should().NotThrowAsync();
    }

    private static async Task<Session> LoadAsync(SumiClasDbContext context, Guid sessionId)
        => await context.Sessions.SingleAsync(session => session.Id == sessionId);

    private static async Task<Guid> SaveSessionAsync(TestDatabase database)
    {
        Session session = SessionMother.With(tokenHash: CURRENT_HASH);
        await using SumiClasDbContext context = database.NewContext();
        context.Sessions.Add(session);
        await context.SaveChangesAsync();
        return session.Id;
    }
}
