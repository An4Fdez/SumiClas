namespace SumiClas.Tests.Schools.Domain;

using SumiClas.Schools.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;

public static class SchoolMother
{
    public static School InLocality(string name, Guid localityId, Guid? postalCodeId = null)
        => With(name, localityId, postalCodeId, null, null);

    public static School WithContact(string name, Guid localityId, Guid postalCodeId, string address, string phone)
        => With(name, localityId, postalCodeId, address, phone);

    private static School With(
        string name,
        Guid localityId,
        Guid? postalCodeId,
        string? address,
        string? phone)
        => new(
            GuidMother.Random(),
            name,
            TextNormalizer.Normalize(name),
            StringMother.Random("code"),
            localityId,
            postalCodeId,
            address,
            phone);
}
