namespace SumiClas.Tests.Sessions.Domain;

using SumiClas.Sessions.Domain;
using SumiClas.Tests.Shared;

public static class RefreshTokenMother
{
    public static RefreshToken With(string tokenHash)
        => new(
            GuidMother.Random(),
            tokenHash,
            UnitTestCase.Now,
            UnitTestCase.Now + RefreshToken.Lifetime,
            null,
            null);
}
