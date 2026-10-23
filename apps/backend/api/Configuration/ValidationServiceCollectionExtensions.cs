namespace SumiClas.Api.Configuration;

public static class ValidationServiceCollectionExtensions
{
    /// <summary>
    /// Turns on the validation attributes of the request records ([StringLength], [Range], [Required]). It lives in this
    /// project, next to the endpoints, because the validation source generator finds the types to validate from the
    /// project that makes the call.
    /// </summary>
    public static IServiceCollection AddSumiClasValidation(this IServiceCollection services)
    {
        services.AddValidation();
        return services;
    }
}
