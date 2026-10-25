namespace SumiClas.WeeklySummaries.Domain;

/// <summary>
/// The writer could not produce the summary: the service is not configured, it kept failing, it timed out or it answered
/// something unusable. It is not a domain error: nobody can fix it by changing the request, so the caller only decides
/// whether to try again later. The message never carries the key nor the content of the request.
/// </summary>
public sealed class WeeklySummaryWriterException(string message, Exception? inner = null) : Exception(message, inner);
