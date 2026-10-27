namespace SumiClas.LostItems.Application;

/// <param name="PhotoUrl">A URL local to the author's device: the server never stores the image.</param>
/// <param name="PendingClaimsCount">How many claims wait for the author; null for anyone else.</param>
/// <param name="MyClaimStatus">The status of the latest claim of the current user; null for the author or if they never claimed.</param>
public sealed record LostItemResponse(
    Guid Id,
    string Scope,
    Guid? ClassId,
    string Status,
    bool Recovered,
    string Description,
    string? PhotoUrl,
    Guid AuthorId,
    string AuthorName,
    string? AuthorRole,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    bool IsAuthor,
    int? PendingClaimsCount,
    string? MyClaimStatus);
