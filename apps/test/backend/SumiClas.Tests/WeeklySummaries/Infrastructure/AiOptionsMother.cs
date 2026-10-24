namespace SumiClas.Tests.WeeklySummaries.Infrastructure;

using SumiClas.Tests.Shared;
using SumiClas.WeeklySummaries.Infrastructure;

public static class AiOptionsMother
{
    public static AiOptions Enabled(
        string? apiKey = null,
        string? model = null,
        int timeoutSeconds = AiOptions.DefaultTimeoutSeconds,
        int maxRetries = AiOptions.DefaultMaxRetries)
        => new()
        {
            ApiKey = apiKey ?? StringMother.Random("key"),
            Model = model ?? StringMother.Random("model"),
            TimeoutSeconds = timeoutSeconds,
            MaxRetries = maxRetries,
        };

    public static AiOptions WithoutKey() => new() { Model = StringMother.Random("model") };
}
