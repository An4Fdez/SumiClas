using SumiClas.WeeklySummaries.Domain;
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

    /// <summary>
    /// Registers the writer of the weekly summary over its own <see cref="HttpClient"/>. Each attempt has its own timeout
    /// (<c>Ai:TimeoutSeconds</c>), so the client's general one is turned off. It is registered even without a key: the
    /// writer only refuses to work when it is asked to, with a <see cref="WeeklySummaryWriterException"/> that the API
    /// answers as a 503, so a week that needs no text (no news, or one already written) is answered without a key.
    /// </summary>
    public static IServiceCollection AddSumiClasWeeklySummaryWriter(this IServiceCollection services)
    {
        services.AddHttpClient<IWeeklySummaryWriter, AnthropicWeeklySummaryWriter>(client =>
        {
            client.BaseAddress = new Uri(AnthropicWeeklySummaryWriter.BaseAddress);
            client.Timeout = Timeout.InfiniteTimeSpan;
        });

        return services;
    }
}
