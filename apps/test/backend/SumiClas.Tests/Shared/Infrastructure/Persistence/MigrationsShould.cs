namespace SumiClas.Tests.Shared.Infrastructure.Persistence;

using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql;
using SumiClas.Shared.Infrastructure.Persistence;

/// <summary>The schema over a real PostgreSQL: the migrations must build it from nothing and match the model.</summary>
[TestFixture]
public class MigrationsShould
{
    [Test]
    public async Task apply_every_migration_to_an_empty_database()
    {
        await using TestDatabase database = await PostgresServer.CreateEmptyDatabaseAsync();
        await using SumiClasDbContext context = database.NewContext();

        await context.Database.MigrateAsync();

        IEnumerable<string> pending = await context.Database.GetPendingMigrationsAsync();
        IEnumerable<string> applied = await context.Database.GetAppliedMigrationsAsync();
        pending.Should().BeEmpty();
        applied.Should().Equal(context.Database.GetMigrations());
    }

    [Test]
    public async Task leave_no_change_of_the_model_without_a_migration()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        await using SumiClasDbContext context = database.NewContext();

        context.Database.HasPendingModelChanges().Should().BeFalse(
            "a change to a mapping needs its migration, and the model snapshot must match the entities.");
    }

    [Test]
    public async Task create_a_table_for_every_entity_of_the_model()
    {
        await using TestDatabase database = await PostgresServer.CreateMigratedDatabaseAsync();
        await using SumiClasDbContext context = database.NewContext();
        List<string> expected = context.Model.GetEntityTypes()
            .Select(entity => entity.GetTableName())
            .OfType<string>()
            .Distinct()
            .ToList();

        List<string> tables = await TablesOfAsync(context);

        expected.Should().NotBeEmpty();
        tables.Should().Contain(expected);
    }

    private static async Task<List<string>> TablesOfAsync(SumiClasDbContext context)
    {
        await using NpgsqlConnection connection = new(context.Database.GetConnectionString());
        await connection.OpenAsync();
        await using NpgsqlCommand command = new(
            "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'",
            connection);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        List<string> tables = [];
        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0));
        }

        return tables;
    }
}
