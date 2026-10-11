using Microsoft.EntityFrameworkCore;
using SumiClas.Users.Domain;
using SumiClas.Users.Infrastructure;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Shared.Infrastructure;
using SumiClas.Shared.Infrastructure.Persistence;

namespace SumiClas.Api.Configuration;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddSumiClasInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SumiClasDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("SumiClasDb")));

        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<SumiClasDbContext>());
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IPasswordHasher, IdentityPasswordHasher>();
        services.AddScoped<IQueryBus, MediatRQueryBus>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddRepositoriesByConvention(typeof(SumiClasDbContext).Assembly);

        return services;
    }
}
