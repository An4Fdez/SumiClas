using SumiClas.Shared.Domain;

namespace SumiClas.Shared.Infrastructure.Persistence;

/// <summary>Converts a domain specification into the common <see cref="IQueryable{T}"/> the repositories run.</summary>
public static class SpecificationConverter<TEntity>
{
    public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> query, ISpecification<TEntity> specification)
        => query.Where(specification.Criteria);
}
