namespace SumiClas.Tests.Shared.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Npgsql;
using SumiClas.Shared.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

/// <summary>
/// The PostgreSQL the persistence tests run against: one container per test run, with a template database that already
/// has every migration applied, from which each test takes its own copy. Only a container that cannot start (no Docker)
/// skips the tests, unless SUMICLAS_REQUIRE_POSTGRES=true (for CI, where a missing server must not pass as a skip);
/// a migration that fails stops the whole run, since that is a defect and not a missing tool.
/// </summary>
public static class PostgresServer
{
    private const string IMAGE = "postgres:17-alpine";
    private const string TEMPLATE = "sumiclas_template";
    private const string REQUIRE_VARIABLE = "SUMICLAS_REQUIRE_POSTGRES";

    private static PostgreSqlContainer? Container;
    private static string? UnavailableReason;

    /// <summary>A database with all the migrations applied, copied from the template; dropped on dispose.</summary>
    public static async Task<TestDatabase> CreateMigratedDatabaseAsync()
        => await CreateDatabaseAsync($"CREATE DATABASE \"{{0}}\" TEMPLATE \"{TEMPLATE}\"");

    /// <summary>An empty database, to prove that the migrations build the schema from nothing.</summary>
    public static async Task<TestDatabase> CreateEmptyDatabaseAsync()
        => await CreateDatabaseAsync("CREATE DATABASE \"{0}\"");

    internal static async Task StartAsync()
    {
        try
        {
            Container = new PostgreSqlBuilder(IMAGE).Build();
            await Container.StartAsync();
        }
        catch (Exception exception) when (Environment.GetEnvironmentVariable(REQUIRE_VARIABLE) != "true")
        {
            UnavailableReason = exception.Message;
            return;
        }

        await ExecuteAsync($"CREATE DATABASE \"{TEMPLATE}\"");
        await using SumiClasDbContext context = ContextOf(TEMPLATE);
        await context.Database.MigrateAsync();
    }

    internal static async Task StopAsync()
    {
        if (Container is not null)
        {
            await Container.DisposeAsync();
        }
    }

    internal static SumiClasDbContext ContextOf(string database)
    {
        DbContextOptions<SumiClasDbContext> options = new DbContextOptionsBuilder<SumiClasDbContext>()
            .UseNpgsql(ConnectionStringOf(database))
            .Options;
        return new SumiClasDbContext(options);
    }

    internal static async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(ConnectionStringOf("postgres"));
        await connection.OpenAsync();
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<TestDatabase> CreateDatabaseAsync(string createSql)
    {
        if (UnavailableReason is not null)
        {
            Assert.Ignore($"PostgreSQL in a Docker container is not available: {UnavailableReason}");
        }

        if (Container is null)
        {
            throw new InvalidOperationException(
                "The PostgreSQL server only starts for the tests in the namespace of PostgresServerSetUp.");
        }

        string name = $"test_{Guid.NewGuid():N}";
        await ExecuteAsync(string.Format(createSql, name));
        return new TestDatabase(name);
    }

    private static string ConnectionStringOf(string database)
        => new NpgsqlConnectionStringBuilder(Container!.GetConnectionString())
        {
            Database = database,
            Pooling = false,
        }.ConnectionString;
}
