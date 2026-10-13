namespace SumiClas.Tests.Shared;

using SumiClas.Shared.Domain;
using SumiClas.Shared.Infrastructure.Persistence;

public static class InMemorySearch
{
    public static IReadOnlyCollection<TEntity> Run<TEntity>(
        IEnumerable<TEntity> entities,
        ISpecification<TEntity> specification)
        => SpecificationConverter<TEntity>.GetQuery(entities.AsQueryable(), specification).ToList();
}
