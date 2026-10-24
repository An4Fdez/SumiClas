namespace SumiClas.Announcements.Application;

/// <summary>What the current user has not read yet: <paramref name="Unread"/> counts every unread announcement and
/// <paramref name="UnreadUrgent"/> the high priority ones among them.</summary>
public sealed record AnnouncementUnreadSummaryResponse(int Unread, int UnreadUrgent);
