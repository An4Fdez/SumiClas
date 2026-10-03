using SumiClas.Shared.Domain;

namespace SumiClas.LostItems.Domain;

public interface ILostItemRepository
{
    Task<LostItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<LostItem>> SearchAsync(ISpecification<LostItem> specification, CancellationToken ct = default);
    Task AddAsync(LostItem lostItem, CancellationToken ct = default);
}
