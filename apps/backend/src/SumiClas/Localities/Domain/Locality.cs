using SumiClas.Shared.Domain;

namespace SumiClas.Localities.Domain;

public sealed class Locality : AggregateRoot
{
    public Guid ProvinceId { get; private set; }
    public string Name { get; private set; }
    public string NormalizedName { get; private set; }

    public Locality(Guid id, Guid provinceId, string name, string normalizedName) : base(id)
    {
        ProvinceId = provinceId;
        Name = name;
        NormalizedName = normalizedName;
    }

    public static Result<Locality> Create(Guid id, Guid provinceId, string name)
    {
        if (id == Guid.Empty)
        {
            return Result<Locality>.Failure(LocalityErrors.IdRequired);
        }
        if (provinceId == Guid.Empty)
        {
            return Result<Locality>.Failure(LocalityErrors.ProvinceRequired);
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<Locality>.Failure(LocalityErrors.NameRequired);
        }

        return Result<Locality>.Success(new Locality(id, provinceId, name.Trim(), TextNormalizer.Normalize(name)));
    }
}
