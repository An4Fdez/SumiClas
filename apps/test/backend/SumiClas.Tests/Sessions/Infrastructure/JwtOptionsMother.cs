namespace SumiClas.Tests.Sessions.Infrastructure;

using SumiClas.Sessions.Infrastructure;
using SumiClas.Tests.Shared;

public static class JwtOptionsMother
{
    public static JwtOptions Create(string issuer, string audience, string signingKey)
        => new() { Issuer = issuer, Audience = audience, SigningKey = signingKey };

    public static JwtOptions Random() => With();

    public static JwtOptions With(string? issuer = null, string? audience = null, string? signingKey = null)
        => Create(
            issuer ?? StringMother.Random("issuer"),
            audience ?? StringMother.Random("audience"),
            signingKey ?? StringMother.Random("signing-key") + "-with-at-least-32-bytes");
}
