using System.Linq.Expressions;

namespace SumiClas.Shared.Domain;

/// <summary>
/// Base for specifications built from optional filters: each <see cref="AddCriteria"/> is combined with AND,
/// and with no criteria added the specification matches every entity.
/// </summary>
public abstract class Specification<TEntity> : ISpecification<TEntity>
{
    private readonly List<SpecificationOrder> _orders = [];

    public Expression<Func<TEntity, bool>> Criteria { get; private set; } = entity => true;

    public IReadOnlyList<SpecificationOrder> Orders => _orders;

    public int? Take { get; private set; }

    protected void AddCriteria(Expression<Func<TEntity, bool>> criteria)
    {
        var parameter = Criteria.Parameters[0];
        var right = new ParameterReplacer(criteria.Parameters[0], parameter).Visit(criteria.Body);
        Criteria = Expression.Lambda<Func<TEntity, bool>>(Expression.AndAlso(Criteria.Body, right), parameter);
    }

    protected void AddOrder<TKey>(Expression<Func<TEntity, TKey>> key, bool descending = false)
        => _orders.Add(new SpecificationOrder(key, descending));

    /// <summary>
    /// Keyset paging over (key, id): orders by the key then the id (both in the same direction),
    /// keeps only what comes after the page boundary and takes at most the page size.
    /// </summary>
    protected void AddKeysetPage<TKey>(
        Expression<Func<TEntity, TKey>> key,
        Expression<Func<TEntity, Guid>> id,
        KeysetPage<TKey> page,
        bool descending)
    {
        AddOrder(key, descending);
        AddOrder(id, descending);
        Take = page.Take;

        if (page.After is null)
        {
            return;
        }

        var parameter = Criteria.Parameters[0];
        var keyBody = new ParameterReplacer(key.Parameters[0], parameter).Visit(key.Body);
        var idBody = new ParameterReplacer(id.Parameters[0], parameter).Visit(id.Body);

        var afterKey = Expression.Property(Expression.Constant(new Box<TKey>(page.After.Key)), nameof(Box<TKey>.Value));
        var afterId = Expression.Property(Expression.Constant(new Box<Guid>(page.After.Id)), nameof(Box<Guid>.Value));

        var keyBeyond = descending ? Expression.LessThan(keyBody, afterKey) : Expression.GreaterThan(keyBody, afterKey);
        var keyTied = Expression.Equal(keyBody, afterKey);
        var idComparison = Expression.Call(idBody, typeof(Guid).GetMethod(nameof(Guid.CompareTo), [typeof(Guid)])!, afterId);
        var idBeyond = descending
            ? Expression.LessThan(idComparison, Expression.Constant(0))
            : Expression.GreaterThan(idComparison, Expression.Constant(0));

        var after = Expression.OrElse(keyBeyond, Expression.AndAlso(keyTied, idBeyond));
        AddCriteria(Expression.Lambda<Func<TEntity, bool>>(after, parameter));
    }

    private sealed class Box<T>(T value)
    {
        public T Value { get; } = value;
    }

    private sealed class ParameterReplacer(ParameterExpression from, Expression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == from ? to : base.VisitParameter(node);
    }
}
