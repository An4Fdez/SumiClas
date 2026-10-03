namespace SumiClas.Shared.Application;

/// <summary>One page of a keyset-paginated list; <paramref name="NextCursor"/> is null when there are no more items.</summary>
public sealed record PagedResponse<T>(IReadOnlyCollection<T> Items, string? NextCursor);
