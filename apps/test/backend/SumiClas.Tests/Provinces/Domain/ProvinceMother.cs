namespace SumiClas.Tests.Provinces.Domain;

using SumiClas.Provinces.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;

public static class ProvinceMother
{
    public static Province Random() => With();

    public static Province With(string? name = null)
    {
        string resolvedName = name ?? StringMother.Random("Province");

        return new(GuidMother.Random(), resolvedName, TextNormalizer.Normalize(resolvedName));
    }

    public static Province WithName(string name) => With(name);
}
