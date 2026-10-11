using SumiClas.Api.Http;
using SumiClas.Shared.Application;

namespace SumiClas.Api.Configuration;

public static class IdentityServiceCollectionExtensions
{
    public static IServiceCollection AddSumiClasIdentity(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserProvider, HeaderUserProvider>();

        return services;
    }
}
