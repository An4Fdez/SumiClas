namespace SumiClas.LostItems.Application;

/// <param name="PhotoUrl">A URL local to the author's device: the server never stores the image.</param>
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
    bool IsAuthor);
