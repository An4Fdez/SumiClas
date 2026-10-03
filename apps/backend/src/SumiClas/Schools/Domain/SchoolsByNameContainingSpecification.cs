using System.Linq.Expressions;
using SumiClas.Shared.Domain;

namespace SumiClas.Schools.Domain;

/// <summary>Case-insensitive "contains" on the name. The base specification only compares for equality.</summary>
public sealed class SchoolsByNameContainingSpecification(string term) : ISpecification<School>
{
    private readonly string _term = term.Trim().ToLowerInvariant();

    public Expression<Func<School, bool>> Criteria => school => school.Name.ToLower().Contains(_term);
}
