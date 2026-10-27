namespace SumiClas.LostItems.Application;

/// <param name="Count">How many claims are waiting for the current user to accept or reject them.</param>
public sealed record LostItemClaimPendingSummaryResponse(int Count);
