using SumiClas.Shared.Domain;

namespace SumiClas.Provinces.Domain;

public sealed class ProvinceSpecification : Specification<Province>
{
    public ProvinceSpecification(
        Guid? id = null,
        string? normalizedName = null,
        IReadOnlyCollection<Guid>? ids = null)
    {
        if (id is { } provinceId) AddCriteria(province => province.Id == provinceId);
        if (normalizedName is not null) AddCriteria(province => province.NormalizedName == normalizedName);
        if (ids is not null)
        {
            var provinceIds = ids.ToArray();
            AddCriteria(province => provinceIds.Contains(province.Id));
        }
    }
}
