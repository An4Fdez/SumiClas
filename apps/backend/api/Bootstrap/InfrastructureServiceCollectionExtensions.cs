using Microsoft.EntityFrameworkCore;
using SumiClas.Shared.Application;
using SumiClas.Shared.Infrastructure;
using SumiClas.Shared.Infrastructure.Persistence;

namespace SumiClas.Api.Bootstrap;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddSumiClasInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SumiClasDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("SumiClasDb")));

        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<SumiClasDbContext>());
        services.AddScoped<IQueryBus, MediatRQueryBus>();
        services.AddRepositoriesByConvention(typeof(SumiClasDbContext).Assembly);

        return services;
    }
}
