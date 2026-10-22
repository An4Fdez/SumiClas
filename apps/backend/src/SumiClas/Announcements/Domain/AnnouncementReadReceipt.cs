using SumiClas.Shared.Domain;

namespace SumiClas.Announcements.Domain;

/// <summary>
/// Child entity of the Announcement aggregate — not an aggregate root itself.
/// Only Announcement (via MarkAsReadBy) can create one; it protects the invariant
/// "at most one receipt per user" internally, since both live in the same aggregate.
/// </summary>
public sealed class AnnouncementReadReceipt : Entity
{
    public Guid UserId { get; private set; }
    public DateTimeOffset ReadAt { get; private set; }

    private AnnouncementReadReceipt(Guid id, Guid userId, DateTimeOffset readAt) : base(id)
    {
        UserId = userId;
        ReadAt = readAt;
    }

    internal static AnnouncementReadReceipt Create(Guid userId, IClock clock)
    {
        if (userId == Guid.Empty)
        {
            throw new AnnouncementExceptions.ReadReceiptUserRequired();
        }

        return new AnnouncementReadReceipt(Guid.NewGuid(), userId, clock.UtcNow);
    }
}
