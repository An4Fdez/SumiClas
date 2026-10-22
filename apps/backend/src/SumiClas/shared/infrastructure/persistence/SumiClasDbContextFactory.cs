using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SumiClas.Shared.Infrastructure.Persistence;

/// <summary>
/// Used only by `dotnet ef` design-time tooling (migrations) — Program.cs does not
/// register SumiClasDbContext via DI yet, so there is no other way for the tooling
/// to construct it. Points at the local docker-compose Postgres (dev-only credentials,
/// see docker-compose.yml); never used at application runtime.
/// </summary>
public sealed class SumiClasDbContextFactory : IDesignTimeDbContextFactory<SumiClasDbContext>
{
    public SumiClasDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<SumiClasDbContext> optionsBuilder = new DbContextOptionsBuilder<SumiClasDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=sumiclas;Username=sumiclas;Password=sumiclas_dev");

        return new SumiClasDbContext(optionsBuilder.Options);
    }
}
