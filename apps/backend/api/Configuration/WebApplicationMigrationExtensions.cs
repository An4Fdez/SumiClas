using Microsoft.EntityFrameworkCore;
using SumiClas.Shared.Infrastructure.Persistence;

namespace SumiClas.Api.Configuration;

public static class WebApplicationMigrationExtensions
{
    /// <summary>
    /// Applies pending EF Core migrations on startup, in every environment. See AGENTS.md
    /// "Base de datos" — this was a deliberate choice, not the safer dev-only default.
    /// </summary>
    public static WebApplication ApplySumiClasMigrations(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        SumiClasDbContext dbContext = scope.ServiceProvider.GetRequiredService<SumiClasDbContext>();
        dbContext.Database.Migrate();

        return app;
    }
}
