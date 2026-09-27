namespace SumiClas.LostItems.Domain;

public interface ILostItemRepository
{
    Task<LostItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<LostItem>> GetByClassIdAsync(Guid classId, CancellationToken ct = default);
    Task<IReadOnlyCollection<LostItem>> GetSchoolWideAsync(CancellationToken ct = default);
    Task AddAsync(LostItem lostItem, CancellationToken ct = default);
}
