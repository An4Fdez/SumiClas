namespace SumiClas.Tests.WeeklySummaries.Infrastructure;

using System.Net;
using System.Net.Http.Headers;

/// <summary>
/// Stands where the network would be: it answers each request with the next reply of the queue and keeps what it was
/// sent. A request with no reply left fails the test instead of waiting for an answer that will never come.
/// </summary>
public sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<Func<CancellationToken, Task<HttpResponseMessage>>> _replies = new();
    private readonly TaskCompletionSource _firstRequest = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public List<RecordedRequest> Requests { get; } = [];

    /// <summary>Completes when the first request has arrived, so a test can act while it is still in flight.</summary>
    public Task FirstRequest => this._firstRequest.Task;

    public FakeHttpMessageHandler Reply(HttpStatusCode status, string body, TimeSpan? retryAfter = null)
    {
        this._replies.Enqueue(_ =>
        {
            HttpResponseMessage response = new(status) { Content = new StringContent(body) };
            response.Headers.RetryAfter = retryAfter is null ? null : new RetryConditionHeaderValue(retryAfter.Value);
            return Task.FromResult(response);
        });
        return this;
    }

    public FakeHttpMessageHandler Fail(Exception exception)
    {
        this._replies.Enqueue(_ => Task.FromException<HttpResponseMessage>(exception));
        return this;
    }

    /// <summary>Never answers: the request only ends when it is cancelled.</summary>
    public FakeHttpMessageHandler Hang()
    {
        this._replies.Enqueue(async ct =>
        {
            await Task.Delay(Timeout.Infinite, ct);
            throw new InvalidOperationException("A hanging request cannot be answered.");
        });
        return this;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        Dictionary<string, string> headers = request.Headers.ToDictionary(
            header => header.Key,
            header => string.Join(",", header.Value));
        string body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(ct);
        this.Requests.Add(new RecordedRequest(request.Method, request.RequestUri, headers, body));
        this._firstRequest.TrySetResult();

        if (!this._replies.TryDequeue(out Func<CancellationToken, Task<HttpResponseMessage>>? reply))
        {
            throw new InvalidOperationException("The writer sent more requests than the replies prepared.");
        }

        return await reply(ct);
    }
}

public sealed record RecordedRequest(
    HttpMethod Method,
    Uri? Uri,
    IReadOnlyDictionary<string, string> Headers,
    string Body);
