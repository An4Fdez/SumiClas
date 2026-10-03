namespace SumiClas.CalendarEvents.Application;

/// <summary>
/// Birthdays carry month and day only: <paramref name="Year"/> is null for them, so a child's age can never be derived.
/// </summary>
public sealed record CalendarEventResponse(
    Guid Id,
    string Scope,
    Guid? ClassId,
    Guid? StudentId,
    string Type,
    string Title,
    int Month,
    int Day,
    int? Year,
    TimeOnly? Time,
    string? Location,
    string? Cost,
    Guid AuthorId,
    string AuthorName,
    string? AuthorRole,
    DateTimeOffset CreatedAt);
