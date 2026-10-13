namespace SumiClas.Tests.Localities.Domain;

using SumiClas.Localities.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;

public static class LocalityMother
{
    public static Locality InProvince(Guid provinceId, string name)
        => new(GuidMother.Random(), provinceId, name, TextNormalizer.Normalize(name));
}
