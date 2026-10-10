using SumiClas.Shared.Domain;

namespace SumiClas.Schools.Domain;

/// <summary>
/// Schools of one locality whose normalized name contains the (normalized) term, ordered by name, optionally capped.
/// The base specification only compares for equality, and a blank term filters nothing.
/// </summary>
public sealed class SchoolsInLocalityByNameSpecification : Specification<School>
{
    public SchoolsInLocalityByNameSpecification(Guid localityId, string? term, int? take = null)
    {
        AddCriteria(school => school.LocalityId == localityId);

        var normalizedTerm = TextNormalizer.Normalize(term);
        if (normalizedTerm.Length > 0)
        {
            AddCriteria(school => school.NormalizedName.Contains(normalizedTerm));
        }

        AddOrder(school => school.NormalizedName);
        AddOrder(school => school.Id);
        if (take is { } count) AddTake(count);
    }
}
