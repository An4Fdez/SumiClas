namespace SumiClas.Tests.Shared.Infrastructure.Persistence;

using SumiClas.Shared.Infrastructure.Persistence;

/// <summary>One database of a test, with contexts to reach it; it is dropped when the test is done with it.</summary>
public sealed class TestDatabase(string name) : IAsyncDisposable
{
    public SumiClasDbContext NewContext() => PostgresServer.ContextOf(name);

    public async ValueTask DisposeAsync()
        => await PostgresServer.ExecuteAsync($"DROP DATABASE IF EXISTS \"{name}\" WITH (FORCE)");
}
