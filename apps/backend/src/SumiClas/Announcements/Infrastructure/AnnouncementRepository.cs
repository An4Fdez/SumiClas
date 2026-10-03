using SumiClas.Announcements.Domain;
using SumiClas.Shared.Infrastructure.Persistence;

namespace SumiClas.Announcements.Infrastructure;

public sealed class AnnouncementRepository(SumiClasDbContext dbContext)
    : BaseRepository<Announcement>(dbContext), IAnnouncementRepository;
