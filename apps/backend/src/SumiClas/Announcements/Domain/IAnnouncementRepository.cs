using SumiClas.Shared.Domain;

namespace SumiClas.Announcements.Domain;

public interface IAnnouncementRepository
{
    Task<Announcement?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<Announcement>> SearchAsync(ISpecification<Announcement> specification, CancellationToken ct = default);
    Task AddAsync(Announcement announcement, CancellationToken ct = default);
    void Remove(Announcement announcement);
}
