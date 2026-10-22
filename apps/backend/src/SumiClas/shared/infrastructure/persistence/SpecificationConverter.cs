using SumiClas.Shared.Domain;

namespace SumiClas.Shared.Infrastructure.Persistence;

/// <summary>Converts a domain specification into the common <see cref="IQueryable{T}"/> the repositories run.</summary>
public static class SpecificationConverter<TEntity>
{
    public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> query, ISpecification<TEntity> specification)
    {
        query = query.Where(specification.Criteria);

        IOrderedQueryable<TEntity>? ordered = null;
        foreach (SpecificationOrder order in specification.Orders)
        {
            string method = (ordered is null, order.Descending) switch
            {
                (true, false) => nameof(Queryable.OrderBy),
                (true, true) => nameof(Queryable.OrderByDescending),
                (false, false) => nameof(Queryable.ThenBy),
                (false, true) => nameof(Queryable.ThenByDescending),
            };

            ordered = (IOrderedQueryable<TEntity>)typeof(Queryable)
                .GetMethods()
                .Single(candidate => candidate.Name == method && candidate.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TEntity), order.Key.ReturnType)
                .Invoke(null, [ordered ?? query, order.Key])!;
        }

        query = ordered ?? query;

        return specification.Take is { } take ? query.Take(take) : query;
    }
}
