using SumiClas.Shared.Domain;

namespace SumiClas.Announcements.Domain;

public sealed class Announcement : AggregateRoot
{
    private readonly List<AnnouncementReadReceipt> _readReceipts = new();
    public IReadOnlyCollection<AnnouncementReadReceipt> ReadReceipts => _readReceipts.AsReadOnly();

    public AnnouncementScope Scope { get; private set; }

    /// <summary>Only set when Scope == Class.</summary>
    public Guid? ClassId { get; private set; }

    /// <summary>Only set when Scope == School: the school the announcement belongs to.</summary>
    public Guid? SchoolId { get; private set; }

    public string Title { get; private set; }
    public string Body { get; private set; }
    public Priority Priority { get; private set; }
    public Guid AuthorId { get; private set; }

    /// <summary>Optional cross-module reference to a CalendarEvents module CalendarEvent.</summary>
    public Guid? RelatedEventId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public Announcement(
        Guid id,
        AnnouncementScope scope,
        Guid? classId,
        Guid? schoolId,
        string title,
        string body,
        Priority priority,
        Guid authorId,
        Guid? relatedEventId,
        DateTimeOffset createdAt) : base(id)
    {
        Scope = scope;
        ClassId = classId;
        SchoolId = schoolId;
        Title = title;
        Body = body;
        Priority = priority;
        AuthorId = authorId;
        RelatedEventId = relatedEventId;
        CreatedAt = createdAt;
    }

    public static Announcement Create(
        Guid id,
        AnnouncementScope scope,
        Guid? classId,
        Guid? schoolId,
        string title,
        string body,
        Priority priority,
        Guid authorId,
        Guid? relatedEventId,
        IClock clock)
    {
        Validate(id, scope, classId, schoolId, title, body, authorId);

        return new Announcement(
            id,
            scope,
            scope == AnnouncementScope.Class ? classId : null,
            scope == AnnouncementScope.School ? schoolId : null,
            title.Trim(),
            body.Trim(),
            priority,
            authorId,
            relatedEventId,
            clock.UtcNow);
    }

    private static void Validate(Guid id, AnnouncementScope scope, Guid? classId, Guid? schoolId, string title, string body, Guid authorId)
    {
        if (id == Guid.Empty)
        {
            throw new AnnouncementExceptions.IdRequired();
        }
        if (scope == AnnouncementScope.Class && (classId is null || classId == Guid.Empty))
        {
            throw new AnnouncementExceptions.ClassRequiredForClassScope();
        }
        if (scope == AnnouncementScope.School && (schoolId is null || schoolId == Guid.Empty))
        {
            throw new AnnouncementExceptions.SchoolRequiredForSchoolScope();
        }
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new AnnouncementExceptions.TitleRequired();
        }
        if (string.IsNullOrWhiteSpace(body))
        {
            throw new AnnouncementExceptions.BodyRequired();
        }
        if (authorId == Guid.Empty)
        {
            throw new AnnouncementExceptions.AuthorRequired();
        }
    }

    /// <summary>Idempotent: if the user already has a receipt, does nothing.</summary>
    public void MarkAsReadBy(Guid userId, IClock clock)
    {
        if (_readReceipts.Any(r => r.UserId == userId))
        {
            return;
        }

        _readReceipts.Add(AnnouncementReadReceipt.Create(userId, clock));
    }
}
