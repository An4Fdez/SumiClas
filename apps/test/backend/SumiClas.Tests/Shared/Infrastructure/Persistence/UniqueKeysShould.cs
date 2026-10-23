namespace SumiClas.Tests.Shared.Infrastructure.Persistence;

using AwesomeAssertions;
using SumiClas.Shared.Application;
using SumiClas.Shared.Infrastructure.Persistence;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Domain;

/// <summary>
/// The unique keys over a real PostgreSQL: a duplicated email or id must come out of the context as the
/// <see cref="DuplicateKeyException"/> that the unit of work turns into a 409, and nothing of the second save may stay.
/// </summary>
[TestFixture]
public class UniqueKeysShould
{
    private const string SHARED_EMAIL = "familia@colegio.es";

    [Test]
    public async Task raise_a_duplicate_key_naming_the_email_index_when_two_users_share_an_email()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        await SaveUserAsync(database, UserMother.WithEmail(SHARED_EMAIL));

        Func<Task> act = () => SaveUserAsync(database, UserMother.WithEmail(SHARED_EMAIL));

        (await act.Should().ThrowAsync<DuplicateKeyException>())
            .Which.ConstraintName.Should().Be("IX_Users_Email");
    }

    [Test]
    public async Task raise_a_duplicate_key_naming_the_primary_key_when_the_same_id_is_added_twice()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        Guid id = Guid.NewGuid();
        await SaveUserAsync(database, UserMother.With(id: id));

        Func<Task> act = () => SaveUserAsync(database, UserMother.With(id: id));

        (await act.Should().ThrowAsync<DuplicateKeyException>())
            .Which.ConstraintName.Should().Be("PK_Users");
    }

    [Test]
    public async Task save_two_users_whose_emails_and_ids_differ()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        await SaveUserAsync(database, UserMother.Random());

        Func<Task> act = () => SaveUserAsync(database, UserMother.Random());

        await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task keep_only_the_first_user_after_a_duplicate_email()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        User first = UserMother.WithEmail(SHARED_EMAIL);
        await SaveUserAsync(database, first);
        try
        {
            await SaveUserAsync(database, UserMother.WithEmail(SHARED_EMAIL));
        }
        catch (DuplicateKeyException)
        {
            // expected: what matters is what the database keeps
        }

        await using SumiClasDbContext context = database.NewContext();
        List<Guid> ids = context.Users.Select(user => user.Id).ToList();
        ids.Should().Equal(first.Id);
    }

    private static async Task SaveUserAsync(TestDatabase database, User user)
    {
        await using SumiClasDbContext context = database.NewContext();
        context.Users.Add(user);
        await context.SaveChangesAsync();
    }
}
