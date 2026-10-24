namespace SumiClas.ClassExpenses.Application;

/// <param name="Pending">Whether the current user takes part in an expense of the class that they have not paid.</param>
/// <param name="Item">What the oldest of those expenses is for; null when nothing is pending.</param>
public sealed record ClassExpensePendingSummaryResponse(bool Pending, string? Item);
