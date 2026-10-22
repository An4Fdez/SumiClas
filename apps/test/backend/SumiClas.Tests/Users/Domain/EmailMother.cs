namespace SumiClas.Tests.Users.Domain;

using SumiClas.Tests.Shared;
using SumiClas.Users.Domain;

public static class EmailMother
{
    public static Email Create(string value) => Email.Create(value);

    public static Email Random() => Create(GuidMother.Random().ToString("N") + "@example.com");
}
