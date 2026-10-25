namespace SumiClas.WeeklySummaries.Domain;

/// <summary>
/// Writes the summary of a class week. The text is plain: one sentence per line, no bullets and no numbering. An
/// implementation talks to a third party, so it may fail or time out; whoever calls it decides what a failure means.
/// </summary>
public interface IWeeklySummaryWriter
{
    Task<string> WriteAsync(WeeklySummaryInput input, CancellationToken ct = default);
}
