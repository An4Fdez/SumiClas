namespace SumiClas.LostItems.Application;

/// <param name="ClaimantName">Full name as the class members list shows it; empty if the user is no longer a member.</param>
/// <param name="ResponseMessage">The author's coordination line; only present once the claim is accepted.</param>
/// <param name="IsMine">Whether the claim is the current user's.</param>
public sealed record LostItemClaimResponse(
    Guid Id,
    Guid ClaimantId,
    string ClaimantName,
    string? ClaimantRole,
    string Message,
    string Status,
    string? ResponseMessage,
    DateTimeOffset CreatedAt,
    bool IsMine);
