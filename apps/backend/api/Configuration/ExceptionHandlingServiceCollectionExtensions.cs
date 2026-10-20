using SumiClas.Api.Http;

namespace SumiClas.Api.Configuration;

public static class ExceptionHandlingServiceCollectionExtensions
{
    /// <summary>Registers the handler that answers domain exceptions; pair it with <c>app.UseExceptionHandler()</c>.</summary>
    public static IServiceCollection AddSumiClasExceptionHandling(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<DomainExceptionHandler>();
        return services;
    }
}
