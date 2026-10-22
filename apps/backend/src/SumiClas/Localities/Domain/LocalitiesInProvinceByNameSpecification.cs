using SumiClas.Shared.Domain;

namespace SumiClas.Localities.Domain;

/// <summary>
/// Localities of one province whose normalized name contains the (normalized) term, ordered by name.
/// The base specification only compares for equality, and a blank term filters nothing.
/// </summary>
public sealed class LocalitiesInProvinceByNameSpecification : Specification<Locality>
{
    public LocalitiesInProvinceByNameSpecification(Guid provinceId, string? term)
    {
        AddCriteria(locality => locality.ProvinceId == provinceId);

        string normalizedTerm = TextNormalizer.Normalize(term);
        if (normalizedTerm.Length > 0)
        {
            AddCriteria(locality => locality.NormalizedName.Contains(normalizedTerm));
        }

        AddOrder(locality => locality.NormalizedName);
        AddOrder(locality => locality.Id);
    }
}
