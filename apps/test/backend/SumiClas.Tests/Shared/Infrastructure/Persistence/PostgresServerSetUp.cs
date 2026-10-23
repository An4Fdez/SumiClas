namespace SumiClas.Tests.Shared.Infrastructure.Persistence;

/// <summary>Starts the PostgreSQL container once for every test of this namespace and stops it at the end.</summary>
[SetUpFixture]
public class PostgresServerSetUp
{
    [OneTimeSetUp]
    public async Task StartTheServer() => await PostgresServer.StartAsync();

    [OneTimeTearDown]
    public async Task StopTheServer() => await PostgresServer.StopAsync();
}
