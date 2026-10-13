namespace SumiClas.Tests.Sessions.Domain;

using SumiClas.Sessions.Domain;
using SumiClas.Tests.Shared;

public static class SessionMother
{
    public static Session With(Guid? userId = null, string? tokenHash = null)
        => new(
            GuidMother.Random(),
            userId ?? GuidMother.Random(),
            UnitTestCase.Now,
            null,
            null,
            [RefreshTokenMother.With(tokenHash ?? "hash-" + GuidMother.Random().ToString("N"))]);
}
