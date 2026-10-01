using System.Linq.Expressions;

namespace SumiClas.Shared.Domain;

/// <summary>
/// Base for specifications built from optional filters: each <see cref="AddCriteria"/> is combined with AND,
/// and with no criteria added the specification matches every entity.
/// </summary>
public abstract class Specification<TEntity> : ISpecification<TEntity>
{
    public Expression<Func<TEntity, bool>> Criteria { get; private set; } = entity => true;

    protected void AddCriteria(Expression<Func<TEntity, bool>> criteria)
    {
        var parameter = Criteria.Parameters[0];
        var right = new ParameterReplacer(criteria.Parameters[0], parameter).Visit(criteria.Body);
        Criteria = Expression.Lambda<Func<TEntity, bool>>(Expression.AndAlso(Criteria.Body, right), parameter);
    }

    private sealed class ParameterReplacer(ParameterExpression from, Expression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == from ? to : base.VisitParameter(node);
    }
}
