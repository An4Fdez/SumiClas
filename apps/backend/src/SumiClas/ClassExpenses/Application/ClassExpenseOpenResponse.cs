namespace SumiClas.ClassExpenses.Application;

/// <summary>An expense of the class that is still open, without saying who has paid it or who owes it.</summary>
/// <param name="Item">What the expense is for.</param>
public sealed record ClassExpenseOpenResponse(string Item);
