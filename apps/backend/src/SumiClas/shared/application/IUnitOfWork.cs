namespace SumiClas.Shared.Application;

public interface IUnitOfWork
{
    bool HasActiveTransaction { get; }
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
