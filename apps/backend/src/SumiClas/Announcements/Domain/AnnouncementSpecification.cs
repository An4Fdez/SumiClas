using SumiClas.Shared.Domain;

namespace SumiClas.Announcements.Domain;

public sealed class AnnouncementSpecification : Specification<Announcement>
{
    /// <param name="createdFrom">Matches announcements created at this moment or later.</param>
    /// <param name="readByUserId">Matches announcements that have a read receipt from this user.</param>
    /// <param name="unreadByUserId">Matches announcements that have no read receipt from this user.</param>
    public AnnouncementSpecification(
        Guid? id = null,
        AnnouncementScope? scope = null,
        Guid? classId = null,
        Guid? schoolId = null,
        string? title = null,
        string? body = null,
        Priority? priority = null,
        Guid? authorId = null,
        Guid? relatedEventId = null,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? createdFrom = null,
        Guid? readByUserId = null,
        Guid? unreadByUserId = null,
        KeysetPage<DateTimeOffset>? page = null)
    {
        if (id is { } announcementId) AddCriteria(announcement => announcement.Id == announcementId);
        if (scope is { } announcementScope) AddCriteria(announcement => announcement.Scope == announcementScope);
        if (classId is { } schoolClass) AddCriteria(announcement => announcement.ClassId == schoolClass);
        if (schoolId is { } school) AddCriteria(announcement => announcement.SchoolId == school);
        if (title is not null) AddCriteria(announcement => announcement.Title == title);
        if (body is not null) AddCriteria(announcement => announcement.Body == body);
        if (priority is { } announcementPriority) AddCriteria(announcement => announcement.Priority == announcementPriority);
        if (authorId is { } author) AddCriteria(announcement => announcement.AuthorId == author);
        if (relatedEventId is { } relatedEvent) AddCriteria(announcement => announcement.RelatedEventId == relatedEvent);
        if (createdAt is { } created) AddCriteria(announcement => announcement.CreatedAt == created);
        if (createdFrom is { } from) AddCriteria(announcement => announcement.CreatedAt >= from);
        if (readByUserId is { } reader) AddCriteria(announcement => announcement.ReadReceipts.Any(receipt => receipt.UserId == reader));
        if (unreadByUserId is { } unreader) AddCriteria(announcement => announcement.ReadReceipts.All(receipt => receipt.UserId != unreader));
        if (page is not null) AddKeysetPage(announcement => announcement.CreatedAt, announcement => announcement.Id, page, descending: true);
    }
}
