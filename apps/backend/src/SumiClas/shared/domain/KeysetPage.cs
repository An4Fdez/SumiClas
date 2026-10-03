namespace SumiClas.Shared.Domain;

/// <summary>The sort position (key and id) of the last item already seen; the next page starts right after it.</summary>
public sealed record KeysetBoundary<TKey>(TKey Key, Guid Id);

/// <summary>A keyset page: at most <paramref name="Take"/> items after <paramref name="After"/> (the start when null).</summary>
public sealed record KeysetPage<TKey>(int Take, KeysetBoundary<TKey>? After = null);
