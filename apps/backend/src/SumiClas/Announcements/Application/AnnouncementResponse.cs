using SumiClas.CalendarEvents.Application;

namespace SumiClas.Announcements.Application;

/// <param name="UpdatedAt">When the author last modified it; null if it was never modified.</param>
/// <param name="Read">Whether the current user has already marked it as read.</param>
/// <param name="IsAuthor">Whether the current user published it, so only they can modify or delete it.</param>
public sealed record AnnouncementResponse(
    Guid Id,
    string Scope,
    Guid? ClassId,
    string Title,
    string Body,
    string Priority,
    Guid AuthorId,
    string AuthorName,
    string? AuthorRole,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    CalendarEventSummaryResponse? RelatedEvent,
    bool Read,
    bool IsAuthor);
