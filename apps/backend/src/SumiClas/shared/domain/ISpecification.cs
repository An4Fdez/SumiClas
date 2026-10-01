using System.Linq.Expressions;

namespace SumiClas.Shared.Domain;

/// <summary>
/// A named, reusable query criterion over an aggregate. Pure domain: it only holds an expression,
/// the persistence layer decides how to run it.
/// </summary>
public interface ISpecification<TEntity>
{
    Expression<Func<TEntity, bool>> Criteria { get; }
}
