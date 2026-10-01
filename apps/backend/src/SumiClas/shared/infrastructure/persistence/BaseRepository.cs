using Microsoft.EntityFrameworkCore;
using SumiClas.Shared.Domain;

namespace SumiClas.Shared.Infrastructure.Persistence;

public abstract class BaseRepository<TEntity>(SumiClasDbContext dbContext)
    where TEntity : AggregateRoot
{
    protected SumiClasDbContext DbContext { get; } = dbContext;

    protected DbSet<TEntity> Set => DbContext.Set<TEntity>();

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Set.FirstOrDefaultAsync(entity => entity.Id == id, ct);

    public async Task<IReadOnlyCollection<TEntity>> SearchAsync(ISpecification<TEntity> specification, CancellationToken ct = default)
        => await BySpecification(specification).ToListAsync(ct);

    public async Task AddAsync(TEntity entity, CancellationToken ct = default)
        => await Set.AddAsync(entity, ct);

    public void Remove(TEntity entity)
        => Set.Remove(entity);

    protected IQueryable<TEntity> Query()
        => Set.AsQueryable();

    protected IQueryable<TEntity> BySpecification(ISpecification<TEntity> specification)
        => SpecificationConverter<TEntity>.GetQuery(Query(), specification);
}
