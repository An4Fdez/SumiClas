using SumiClas.Shared.Domain;

namespace SumiClas.Shared.Application;

public sealed record PageRequest(int Limit, PageCursor? After);

public static class Pagination
{
    public const int DefaultLimit = 20;
    public const int MaxLimit = 50;

    /// <summary>Validates the page parameters of a request; a null cursor means the first page.</summary>
    public static PageRequest Resolve(int? limit, string? cursor)
    {
        int resolvedLimit = limit ?? DefaultLimit;
        if (resolvedLimit is < 1 or > MaxLimit)
        {
            throw new PaginationExceptions.InvalidLimit();
        }

        if (cursor is null)
        {
            return new PageRequest(resolvedLimit, null);
        }

        return PageCursor.TryDecode(cursor, out PageCursor? decoded)
            ? new PageRequest(resolvedLimit, decoded)
            : throw new PaginationExceptions.InvalidCursor();
    }

    public static readonly IComparer<Guid> IdComparer = Comparer<Guid>.Create(CompareIds);

    /// <summary>The page a branch asks the repository for: one extra item tells whether there is a next page.</summary>
    public static KeysetPage<DateTimeOffset> ByCreatedAt(PageRequest request)
    {
        if (request.After is null)
        {
            return new KeysetPage<DateTimeOffset>(request.Limit + 1);
        }

        if (request.After.Key < 0 || request.After.Key > DateTimeOffset.MaxValue.Ticks)
        {
            throw new PaginationExceptions.InvalidCursor();
        }

        KeysetBoundary<DateTimeOffset> boundary = new KeysetBoundary<DateTimeOffset>(new DateTimeOffset(request.After.Key, TimeSpan.Zero), request.After.Id);
        return new KeysetPage<DateTimeOffset>(request.Limit + 1, boundary);
    }

    public static KeysetPage<DateOnly> ByDate(PageRequest request)
    {
        if (request.After is null)
        {
            return new KeysetPage<DateOnly>(request.Limit + 1);
        }

        if (request.After.Key < DateOnly.MinValue.DayNumber || request.After.Key > DateOnly.MaxValue.DayNumber)
        {
            throw new PaginationExceptions.InvalidCursor();
        }

        KeysetBoundary<DateOnly> boundary = new KeysetBoundary<DateOnly>(DateOnly.FromDayNumber((int)request.After.Key), request.After.Id);
        return new KeysetPage<DateOnly>(request.Limit + 1, boundary);
    }

    public static PageCursor CursorOf(DateTimeOffset createdAt, Guid id) => new(createdAt.UtcTicks, id);

    public static PageCursor CursorOf(DateOnly date, Guid id) => new(date.DayNumber, id);

    /// <summary>
    /// Compares ids in the order PostgreSQL sorts uuid values (bytes in textual order), which differs from
    /// <see cref="Guid.CompareTo(Guid)"/>; in-memory merging must use it to agree with the database ordering.
    /// </summary>
    public static int CompareIds(Guid left, Guid right)
        => string.CompareOrdinal(left.ToString("D"), right.ToString("D"));

    /// <summary>
    /// Cuts an already ordered list (merged from one or several branches, each fetched with limit + 1) into one page.
    /// The next cursor is only set when more than <paramref name="limit"/> items existed.
    /// </summary>
    public static (IReadOnlyList<T> Items, string? NextCursor) Cut<T>(IReadOnlyList<T> ordered, int limit, Func<T, PageCursor> cursorOf)
    {
        List<T> items = ordered.Take(limit).ToList();
        string? next = ordered.Count > limit ? cursorOf(items[^1]).Encode() : null;
        return (items, next);
    }
}
