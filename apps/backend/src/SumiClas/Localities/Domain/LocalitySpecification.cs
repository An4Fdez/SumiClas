using SumiClas.Shared.Domain;

namespace SumiClas.Localities.Domain;

public sealed class LocalitySpecification : Specification<Locality>
{
    public LocalitySpecification(
        Guid? id = null,
        Guid? provinceId = null,
        string? normalizedName = null,
        IReadOnlyCollection<Guid>? ids = null)
    {
        if (id is { } localityId) AddCriteria(locality => locality.Id == localityId);
        if (provinceId is { } province) AddCriteria(locality => locality.ProvinceId == province);
        if (normalizedName is not null) AddCriteria(locality => locality.NormalizedName == normalizedName);
        if (ids is not null)
        {
            Guid[] localityIds = ids.ToArray();
            AddCriteria(locality => localityIds.Contains(locality.Id));
        }
    }
}
