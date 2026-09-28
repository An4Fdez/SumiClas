using Microsoft.EntityFrameworkCore;
using SumiClas.Shared.Infrastructure.Persistence;

namespace SumiClas.Api.Bootstrap;

public static class WebApplicationMigrationExtensions
{
    /// <summary>
    /// Applies pending EF Core migrations on startup, in every environment. See AGENTS.md
    /// "Base de datos" — this was a deliberate choice, not the safer dev-only default.
    /// </summary>
    public static WebApplication ApplySumiClasMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SumiClasDbContext>();
        dbContext.Database.Migrate();

        return app;
    }
}
