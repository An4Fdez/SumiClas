namespace SumiClas.LostItems.Domain;

public interface ILostItemRepository
{
    Task AddAsync(LostItem lostItem, CancellationToken ct = default);
}
