namespace SumiClas.WeeklySummaries.Domain;

/// <summary>
/// An announcement of the week as the writer sees it: its title, whether it is urgent and its body (which the prompt
/// builder cleans and cuts to a short snippet), never its author.
/// </summary>
public sealed record WeeklySummaryAnnouncement(string Title, bool Urgent, string Body);
