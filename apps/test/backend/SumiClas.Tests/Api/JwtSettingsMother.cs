namespace SumiClas.Tests.Api;

using SumiClas.Sessions.Infrastructure;
using SumiClas.Tests.Sessions.Infrastructure;

public static class JwtSettingsMother
{
    public const string ISSUER_KEY = "Jwt:Issuer";
    public const string AUDIENCE_KEY = "Jwt:Audience";
    public const string SIGNING_KEY_KEY = "Jwt:SigningKey";

    public static Dictionary<string, string?> With(
        string? issuer = null,
        string? audience = null,
        string? signingKey = null)
        => From(JwtOptionsMother.With(issuer, audience, signingKey));

    public static Dictionary<string, string?> From(JwtOptions options)
        => new()
        {
            [ISSUER_KEY] = options.Issuer,
            [AUDIENCE_KEY] = options.Audience,
            [SIGNING_KEY_KEY] = options.SigningKey,
        };

    public static Dictionary<string, string?> WithoutSigningKey() => Without(SIGNING_KEY_KEY);

    public static Dictionary<string, string?> WithoutIssuer() => Without(ISSUER_KEY);

    public static Dictionary<string, string?> WithoutAudience() => Without(AUDIENCE_KEY);

    private static Dictionary<string, string?> Without(string key)
    {
        Dictionary<string, string?> settings = From(JwtOptionsMother.Random());
        settings.Remove(key);

        return settings;
    }
}
