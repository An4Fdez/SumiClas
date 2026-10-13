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

    public static Result<Province> Create(Guid id, string name)
    {
        if (id == Guid.Empty)
        {
            return Result<Province>.Failure(ProvinceErrors.IdRequired);
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<Province>.Failure(ProvinceErrors.NameRequired);
        }

        return Result<Province>.Success(new Province(id, name.Trim(), TextNormalizer.Normalize(name)));
    }
}
