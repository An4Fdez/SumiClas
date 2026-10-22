namespace SumiClas.Tests.Sessions.Domain;

using SumiClas.Sessions.Domain;
using SumiClas.Tests.Shared;

public static class AccessTokenMother
{
    public static AccessToken Create(string value, DateTimeOffset expiresAt)
        => AccessToken.Create(value, expiresAt);

    public static AccessToken With(string value) => Create(value, UnitTestCase.Now + AccessToken.Lifetime);
}
