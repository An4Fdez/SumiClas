using Microsoft.EntityFrameworkCore;
using SumiClas.Shared.Infrastructure.Persistence;

namespace SumiClas.Api.Bootstrap;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddSumiClasInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SumiClasDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("SumiClasDb")));

        return services;
    }
}
