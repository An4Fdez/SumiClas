using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace SumiClas.Api.Configuration;

public static class AuthenticationServiceCollectionExtensions
{
    /// <summary>
    /// Bearer authentication plus a fallback policy that requires a signed-in user on every endpoint, so an endpoint is
    /// protected unless it opts out with <c>AllowAnonymous()</c>. Needs <c>AddSumiClasJwtOptions</c> for the signing options.
    /// </summary>
    internal static IServiceCollection AddSumiClasAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>());
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        return services;
    }
}
