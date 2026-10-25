using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using SumiClas.WeeklySummaries.Domain;

namespace SumiClas.WeeklySummaries.Infrastructure;

/// <summary>
/// Writes the summary with the Messages API of Anthropic over a plain <see cref="HttpClient"/>. Every attempt has its own
/// timeout; a timeout, a network failure, a 408, a 429 and any 5xx are tried again up to <see cref="AiOptions.MaxRetries"/>
/// times with a growing pause, and anything else (a rejected key, a bad request, a refusal, a cut or empty answer) fails
/// at once. Thinking is off: the summary is a few sentences and the budget of tokens is all for the text.
/// </summary>
public sealed class AnthropicWeeklySummaryWriter : IWeeklySummaryWriter
{
    public const string BaseAddress = "https://api.anthropic.com/";
    public const string MessagesPath = "v1/messages";
    public const string ApiVersion = "2023-06-01";
    public const string ApiKeyHeader = "x-api-key";
    public const string VersionHeader = "anthropic-version";
    public const int MaxOutputTokens = 1024;

    public static readonly TimeSpan BaseRetryDelay = TimeSpan.FromMilliseconds(500);
    public static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(10);

    private const string JsonContentType = "application/json";
    private const string UserRole = "user";
    private const string ThinkingOff = "disabled";
    private const string TextBlock = "text";
    private const string RefusalStop = "refusal";
    private const string MaxTokensStop = "max_tokens";
    private const int ServerErrorFrom = 500;
    private const int DelayGrowth = 2;

    private readonly HttpClient _http;
    private readonly AiOptions _options;
    private readonly Func<TimeSpan, CancellationToken, Task> _delay;

    public AnthropicWeeklySummaryWriter(
        HttpClient http,
        IOptions<AiOptions> options,
        Func<TimeSpan, CancellationToken, Task>? delay = null)
    {
        _http = http;
        _options = options.Value;
        _delay = delay ?? ((pause, ct) => Task.Delay(pause, ct));
    }

    public async Task<string> WriteAsync(WeeklySummaryInput input, CancellationToken ct = default)
    {
        if (!_options.IsEnabled)
        {
            throw new WeeklySummaryWriterException("The AI service is not configured (Ai:ApiKey is empty).");
        }

        string body = Serialize(WeeklySummaryPromptBuilder.Build(input));
        for (int attempt = 0; ; attempt++)
        {
            Attempt result = await TryOnceAsync(body, ct);
            if (result.Text is not null)
            {
                return result.Text;
            }

            if (!result.Retryable || attempt >= _options.MaxRetries)
            {
                throw new WeeklySummaryWriterException(result.Failure, result.Inner);
            }

            await _delay(PauseBefore(attempt, result.RetryAfter), ct);
        }
    }

    private string Serialize(WeeklySummaryPrompt prompt)
        => JsonSerializer.Serialize(new MessagesRequest(
            _options.Model,
            MaxOutputTokens,
            prompt.System,
            [new RequestMessage(UserRole, prompt.User)],
            new Thinking(ThinkingOff)));

    private async Task<Attempt> TryOnceAsync(string body, CancellationToken ct)
    {
        using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));
        try
        {
            using HttpRequestMessage request = new(HttpMethod.Post, MessagesPath)
            {
                Content = new StringContent(body, Encoding.UTF8, JsonContentType),
            };
            request.Headers.Add(ApiKeyHeader, _options.ApiKey);
            request.Headers.Add(VersionHeader, ApiVersion);

            using HttpResponseMessage response = await _http.SendAsync(request, timeout.Token);
            string content = await response.Content.ReadAsStringAsync(timeout.Token);
            return response.IsSuccessStatusCode ? Read(content) : Rejected(response, content);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return Attempt.Retry($"The AI service did not answer in {_options.TimeoutSeconds} seconds.", null, null);
        }
        catch (HttpRequestException exception)
        {
            return Attempt.Retry("The AI service could not be reached.", null, exception);
        }
    }

    private static Attempt Read(string content)
    {
        MessagesResponse? answer;
        try
        {
            answer = JsonSerializer.Deserialize<MessagesResponse>(content);
        }
        catch (JsonException exception)
        {
            return Attempt.Fail("The AI service answered something that is not a message.", exception);
        }

        if (answer is null)
        {
            return Attempt.Fail("The AI service answered something that is not a message.", null);
        }

        if (answer.StopReason == RefusalStop)
        {
            return Attempt.Fail("The AI service refused to write the summary.", null);
        }

        if (answer.StopReason == MaxTokensStop)
        {
            return Attempt.Fail("The AI service cut the summary before the end.", null);
        }

        string text = string.Concat(
            (answer.Content ?? []).Where(block => block.Type == TextBlock).Select(block => block.Text)).Trim();
        return text.Length == 0
            ? Attempt.Fail("The AI service answered without text.", null)
            : Attempt.Success(text);
    }

    private static Attempt Rejected(HttpResponseMessage response, string content)
    {
        int status = (int)response.StatusCode;
        string failure = $"The AI service answered {status}{ErrorType(content)}.";
        bool retryable = response.StatusCode is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests
            || status >= ServerErrorFrom;
        return retryable ? Attempt.Retry(failure, response.Headers.RetryAfter?.Delta, null) : Attempt.Fail(failure, null);
    }

    private static string ErrorType(string content)
    {
        try
        {
            string? type = JsonSerializer.Deserialize<ErrorResponse>(content)?.Error?.Type;
            return string.IsNullOrWhiteSpace(type) ? string.Empty : $" ({type})";
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }

    private static TimeSpan PauseBefore(int attempt, TimeSpan? retryAfter)
    {
        TimeSpan pause = retryAfter ?? BaseRetryDelay * Math.Pow(DelayGrowth, attempt);
        return pause < TimeSpan.Zero ? TimeSpan.Zero : pause > MaxRetryDelay ? MaxRetryDelay : pause;
    }

    private sealed record Attempt(string? Text, bool Retryable, string Failure, TimeSpan? RetryAfter, Exception? Inner)
    {
        public static Attempt Success(string text) => new(text, false, string.Empty, null, null);

        public static Attempt Fail(string failure, Exception? inner) => new(null, false, failure, null, inner);

        public static Attempt Retry(string failure, TimeSpan? retryAfter, Exception? inner)
            => new(null, true, failure, retryAfter, inner);
    }

    private sealed record MessagesRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("max_tokens")] int MaxTokens,
        [property: JsonPropertyName("system")] string System,
        [property: JsonPropertyName("messages")] IReadOnlyList<RequestMessage> Messages,
        [property: JsonPropertyName("thinking")] Thinking Thinking);

    private sealed record RequestMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content);

    private sealed record Thinking([property: JsonPropertyName("type")] string Type);

    private sealed record MessagesResponse(
        [property: JsonPropertyName("content")] IReadOnlyList<ContentBlock>? Content,
        [property: JsonPropertyName("stop_reason")] string? StopReason);

    private sealed record ContentBlock(
        [property: JsonPropertyName("type")] string? Type,
        [property: JsonPropertyName("text")] string? Text);

    private sealed record ErrorResponse([property: JsonPropertyName("error")] ErrorBody? Error);

    private sealed record ErrorBody([property: JsonPropertyName("type")] string? Type);
}
