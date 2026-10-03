using System.Linq.Expressions;
using SumiClas.Shared.Domain;

namespace SumiClas.Users.Domain;

/// <summary>Needed because <see cref="UserSpecification"/> only filters by a single id.</summary>
public sealed class UsersByIdsSpecification(IReadOnlyCollection<Guid> ids) : ISpecification<User>
{
    private readonly Guid[] _ids = ids.ToArray();

    public Expression<Func<User, bool>> Criteria => user => _ids.Contains(user.Id);
}
