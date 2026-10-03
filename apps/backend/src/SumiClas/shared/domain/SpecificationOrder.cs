using System.Linq.Expressions;

namespace SumiClas.Shared.Domain;

public sealed record SpecificationOrder(LambdaExpression Key, bool Descending);
