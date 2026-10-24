namespace SumiClas.LostItems.Application;

/// <param name="Count">How many lost and found items of the school and of the class are still unresolved.</param>
public sealed record LostItemRecentSummaryResponse(int Count);
