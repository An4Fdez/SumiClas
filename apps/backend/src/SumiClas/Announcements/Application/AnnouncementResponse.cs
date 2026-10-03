using SumiClas.CalendarEvents.Application;

namespace SumiClas.Announcements.Application;

/// <param name="Read">Whether the current user has already marked it as read.</param>
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
    CalendarEventSummaryResponse? RelatedEvent,
    bool Read);
