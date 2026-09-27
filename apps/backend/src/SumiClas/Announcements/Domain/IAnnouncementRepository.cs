namespace SumiClas.Announcements.Domain;

public interface IAnnouncementRepository
{
    Task<Announcement?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<Announcement>> GetByClassIdAsync(Guid classId, CancellationToken ct = default);
    Task<IReadOnlyCollection<Announcement>> GetSchoolWideAsync(CancellationToken ct = default);
    Task AddAsync(Announcement announcement, CancellationToken ct = default);
}
