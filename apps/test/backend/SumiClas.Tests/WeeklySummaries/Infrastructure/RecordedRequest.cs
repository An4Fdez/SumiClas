namespace SumiClas.Tests.WeeklySummaries.Infrastructure;

public sealed record RecordedRequest(
    HttpMethod Method,
    Uri? Uri,
    IReadOnlyDictionary<string, string> Headers,
    string Body);
