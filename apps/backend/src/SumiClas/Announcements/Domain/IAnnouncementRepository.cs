namespace SumiClas.Announcements.Domain;

public interface IAnnouncementRepository
{
    Task AddAsync(Announcement announcement, CancellationToken ct = default);
}
