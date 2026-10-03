namespace SumiClas.Announcements.Domain;

public interface IAnnouncementRepository
{
    Task<Announcement?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Announcement announcement, CancellationToken ct = default);
}
