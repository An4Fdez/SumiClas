namespace SumiClas.WeeklySummaries.Infrastructure;

/// <summary>
/// Settings of the AI service that writes the weekly summaries. The key is a secret: it comes only from configuration
/// (User Secrets in development, <c>Ai__ApiKey</c> elsewhere) and never from a file in the repository. Without it the
/// feature is off and the rest of the application works as usual.
/// </summary>
public sealed class AiOptions
{
    public const string SectionName = "Ai";
    public const string DefaultModel = "claude-haiku-5-5";
    public const int DefaultTimeoutSeconds = 30;
    public const int DefaultMaxRetries = 2;
    public const int MaxTimeoutSeconds = 120;
    public const int MaxRetriesAllowed = 5;

    public string ApiKey { get; init; } = string.Empty;
    public string Model { get; init; } = DefaultModel;
    public int TimeoutSeconds { get; init; } = DefaultTimeoutSeconds;
    public int MaxRetries { get; init; } = DefaultMaxRetries;

    public bool IsEnabled => !string.IsNullOrWhiteSpace(ApiKey);
}
