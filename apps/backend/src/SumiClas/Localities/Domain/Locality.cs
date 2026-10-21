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

    public static Locality Create(Guid id, Guid provinceId, string name)
    {
        if (id == Guid.Empty)
        {
            throw new LocalityExceptions.IdRequired();
        }
        if (provinceId == Guid.Empty)
        {
            throw new LocalityExceptions.ProvinceRequired();
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new LocalityExceptions.NameRequired();
        }

        return new Locality(id, provinceId, name.Trim(), TextNormalizer.Normalize(name));
    }
}
