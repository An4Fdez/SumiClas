using SumiClas.LostItems.Domain;
using SumiClas.Shared.Infrastructure.Persistence;

namespace SumiClas.LostItems.Infrastructure;

public sealed class LostItemRepository(SumiClasDbContext dbContext)
    : BaseRepository<LostItem>(dbContext), ILostItemRepository;
