namespace SumiClas.Tests.Api;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

public static class OpenApiRegistration
{
    /// <summary>
    /// Registers the OpenAPI services by reflection: a direct call makes the OpenAPI source generator emit
    /// interceptors, which only the Web SDK enables, and this test project is not a web project.
    /// </summary>
    public static void AddOpenApi(IServiceCollection services, Action<OpenApiOptions> configure)
    {
        MethodInfo addOpenApi = typeof(OpenApiServiceCollectionExtensions).GetMethod(
            nameof(OpenApiServiceCollectionExtensions.AddOpenApi),
            [typeof(IServiceCollection), typeof(Action<OpenApiOptions>)])!;
        addOpenApi.Invoke(null, [services, configure]);
    }
}
