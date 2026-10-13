namespace SumiClas.Tests.PostalCodes.Domain;

using SumiClas.PostalCodes.Domain;
using SumiClas.Tests.Shared;

public static class PostalCodeMother
{
    public static PostalCode Random() => With();

    public static PostalCode With(Guid? localityId = null)
        => new(
            GuidMother.Random(),
            localityId ?? GuidMother.Random(),
            System.Random.Shared.Next(10000, 99999).ToString());
}
