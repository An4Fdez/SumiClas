using Microsoft.Extensions.DependencyInjection.Extensions;
using SumiClas.Api.Http;
using SumiClas.Shared.Application;

namespace SumiClas.Api.Configuration;

public static class ApiSecurityServiceCollectionExtensions
{
    /// <summary>
    /// Everything that only exists to receive HTTP requests: the current-user provider, the refresh cookie options,
    /// bearer authentication with the fallback policy, and the OpenAPI security. Call it after
    /// <c>AddSumiClasInfrastructure</c> (or <c>AddSumiClasJwtOptions</c>): JwtBearer reads the already-validated <c>JwtOptions</c>.
    /// </summary>
    public static IServiceCollection AddSumiClasApiSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserProvider, JwtUserProvider>();
        services.TryAddSingleton(TimeProvider.System);
        services.AddOptions<RefreshCookieOptions>()
            .Bind(configuration.GetSection(RefreshCookieOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Name), "Jwt:RefreshCookie:Name es obligatorio.")
            .Validate(
                o => !string.IsNullOrWhiteSpace(o.Path) && o.Path.StartsWith('/'),
                "Jwt:RefreshCookie:Path debe empezar por \"/\".")
            .Validate(
                o => RefreshCookie.TryParseSameSite(o.SameSite, out _),
                "Jwt:RefreshCookie:SameSite debe ser Strict, Lax o None.")
            .Validate(
                o => !RefreshCookie.TryParseSameSite(o.SameSite, out var sameSite) || sameSite != SameSiteMode.None || o.Secure,
                "Jwt:RefreshCookie:SameSite=None exige Jwt:RefreshCookie:Secure=true.")
            .ValidateOnStart();

        return services
            .AddSumiClasAuthentication()
            .AddSumiClasOpenApiSecurity();
    }
}
