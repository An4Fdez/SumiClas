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

    public static Result<Announcement> Create(
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
        var validation = Validate(id, scope, classId, schoolId, title, body, authorId);
        if (validation.IsFailure)
        {
            return Result<Announcement>.Failure(validation.Error!);
        }

        var announcement = new Announcement(
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

        return Result<Announcement>.Success(announcement);
    }

    private static Result Validate(Guid id, AnnouncementScope scope, Guid? classId, Guid? schoolId, string title, string body, Guid authorId)
    {
        if (id == Guid.Empty)
        {
            return Result.Failure(AnnouncementErrors.IdRequired);
        }
        if (scope == AnnouncementScope.Class && (classId is null || classId == Guid.Empty))
        {
            return Result.Failure(AnnouncementErrors.ClassRequiredForClassScope);
        }
        if (scope == AnnouncementScope.School && (schoolId is null || schoolId == Guid.Empty))
        {
            return Result.Failure(AnnouncementErrors.SchoolRequiredForSchoolScope);
        }
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure(AnnouncementErrors.TitleRequired);
        }
        if (string.IsNullOrWhiteSpace(body))
        {
            return Result.Failure(AnnouncementErrors.BodyRequired);
        }
        if (authorId == Guid.Empty)
        {
            return Result.Failure(AnnouncementErrors.AuthorRequired);
        }
        return Result.Success();
    }

    /// <summary>Idempotent: if the user already has a receipt, does nothing.</summary>
    public Result MarkAsReadBy(Guid userId, IClock clock)
    {
        if (_readReceipts.Any(r => r.UserId == userId))
        {
            return Result.Success();
        }

        var receipt = AnnouncementReadReceipt.Create(userId, clock);
        if (receipt.IsFailure)
        {
            return Result.Failure(receipt.Error!);
        }

        _readReceipts.Add(receipt.Value);
        return Result.Success();
    }
}
