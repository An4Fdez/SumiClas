using System.Text;
using Microsoft.EntityFrameworkCore;
using SumiClas.Sessions.Domain;
using SumiClas.Sessions.Infrastructure;
using SumiClas.Users.Domain;
using SumiClas.Users.Infrastructure;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Shared.Infrastructure;
using SumiClas.Shared.Infrastructure.Persistence;

namespace SumiClas.Api.Configuration;

public static class InfrastructureServiceCollectionExtensions
{
    private const int MinimumSigningKeyBytes = 32;

    private const string SigningKeyHelp =
        "Defínela con dotnet user-secrets set \"Jwt:SigningKey\" \"<clave de al menos 32 caracteres>\" " +
        "o con la variable de entorno Jwt__SigningKey.";

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
        services.AddSumiClasJwtOptions(configuration);
        services.AddSingleton<IAccessTokenGenerator, JwtAccessTokenGenerator>();
        services.AddSingleton<IRefreshTokenGenerator, RefreshTokenGenerator>();

        return services;
    }

    public static IServiceCollection AddSumiClasJwtOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(
                o => !string.IsNullOrWhiteSpace(o.SigningKey),
                $"Falta la clave de firma de los tokens (Jwt:SigningKey). {SigningKeyHelp}")
            .Validate(
                o => string.IsNullOrWhiteSpace(o.SigningKey) || Encoding.UTF8.GetByteCount(o.SigningKey) >= MinimumSigningKeyBytes,
                $"Jwt:SigningKey debe tener al menos {MinimumSigningKeyBytes} bytes (32 caracteres). {SigningKeyHelp}")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Issuer), "Jwt:Issuer es obligatorio.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Audience), "Jwt:Audience es obligatorio.")
            .ValidateOnStart();

        return services;
    }
}
