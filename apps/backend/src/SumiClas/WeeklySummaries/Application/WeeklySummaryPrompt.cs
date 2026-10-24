namespace SumiClas.WeeklySummaries.Application;

/// <param name="System">What the model is told once: who it is, how to write and what never to do.</param>
/// <param name="User">The data of the week, between <c>&lt;datos&gt;</c> tags.</param>
public sealed record WeeklySummaryPrompt(string System, string User);
