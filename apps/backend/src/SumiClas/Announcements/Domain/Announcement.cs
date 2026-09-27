using SumiClas.Shared.Domain;

namespace SumiClas.Announcements.Domain;

public sealed class Announcement : AggregateRoot
{
    private readonly List<AnnouncementReadReceipt> _readReceipts = new();
    public IReadOnlyCollection<AnnouncementReadReceipt> ReadReceipts => _readReceipts.AsReadOnly();

    public AnnouncementScope Scope { get; private set; }

    /// <summary>Only set when Scope == Class.</summary>
    public Guid? ClassId { get; private set; }

    public string Title { get; private set; }
    public string Body { get; private set; }
    public Priority Priority { get; private set; }
    public Guid AuthorId { get; private set; }

    /// <summary>Optional cross-module reference to a CalendarEvents module CalendarEvent.</summary>
    public Guid? RelatedEventId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    private Announcement(
        Guid id,
        AnnouncementScope scope,
        Guid? classId,
        string title,
        string body,
        Priority priority,
        Guid authorId,
        Guid? relatedEventId,
        DateTimeOffset createdAt) : base(id)
    {
        Scope = scope;
        ClassId = classId;
        Title = title;
        Body = body;
        Priority = priority;
        AuthorId = authorId;
        RelatedEventId = relatedEventId;
        CreatedAt = createdAt;
    }

    public static Result<Announcement> Create(
        AnnouncementScope scope,
        Guid? classId,
        string title,
        string body,
        Priority priority,
        Guid authorId,
        Guid? relatedEventId,
        IClock clock)
    {
        if (scope == AnnouncementScope.Class && (classId is null || classId == Guid.Empty))
        {
            return Result<Announcement>.Failure(AnnouncementErrors.ClassRequiredForClassScope);
        }
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result<Announcement>.Failure(AnnouncementErrors.TitleRequired);
        }
        if (string.IsNullOrWhiteSpace(body))
        {
            return Result<Announcement>.Failure(AnnouncementErrors.BodyRequired);
        }
        if (authorId == Guid.Empty)
        {
            return Result<Announcement>.Failure(AnnouncementErrors.AuthorRequired);
        }

        var announcement = new Announcement(
            Guid.NewGuid(),
            scope,
            scope == AnnouncementScope.Class ? classId : null,
            title.Trim(),
            body.Trim(),
            priority,
            authorId,
            relatedEventId,
            clock.UtcNow);

        return Result<Announcement>.Success(announcement);
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
