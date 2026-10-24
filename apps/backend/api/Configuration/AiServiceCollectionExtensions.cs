using SumiClas.WeeklySummaries.Infrastructure;

namespace SumiClas.Api.Configuration;

public static class AiServiceCollectionExtensions
{
    /// <summary>
    /// Binds and validates the AI settings. A missing key is not an error: it only turns the weekly summary off. The
    /// other settings are checked even then, so a typo is found when the application starts and not when it is used.
    /// </summary>
    public static IServiceCollection AddSumiClasAiOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AiOptions>()
            .Bind(configuration.GetSection(AiOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Model), "Ai:Model es obligatorio.")
            .Validate(
                o => o.TimeoutSeconds is >= 1 and <= AiOptions.MaxTimeoutSeconds,
                $"Ai:TimeoutSeconds debe estar entre 1 y {AiOptions.MaxTimeoutSeconds}.")
            .Validate(
                o => o.MaxRetries is >= 0 and <= AiOptions.MaxRetriesAllowed,
                $"Ai:MaxRetries debe estar entre 0 y {AiOptions.MaxRetriesAllowed}.")
            .ValidateOnStart();

        return services;
    }
}
