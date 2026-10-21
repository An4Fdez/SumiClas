using SumiClas.Shared.Domain;

namespace SumiClas.Provinces.Domain;

public sealed class Province : AggregateRoot
{
    public string Name { get; private set; }
    public string NormalizedName { get; private set; }

    public Province(Guid id, string name, string normalizedName) : base(id)
    {
        Name = name;
        NormalizedName = normalizedName;
    }

    public static Province Create(Guid id, string name)
    {
        if (id == Guid.Empty)
        {
            throw new ProvinceExceptions.IdRequired();
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ProvinceExceptions.NameRequired();
        }

        return new Province(id, name.Trim(), TextNormalizer.Normalize(name));
    }
}
