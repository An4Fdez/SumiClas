namespace SumiClas.Tests.Api;

using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SumiClas.Sessions.Domain;
using System.Text;

public static class JwtTokenMother
{
    public static string Create(
        Guid userId,
        string issuer,
        string audience,
        string signingKey,
        string algorithm,
        string type,
        DateTimeOffset issuedAt)
    {
        DateTime now = issuedAt.UtcDateTime;
        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Audience = audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = now + AccessToken.Lifetime,
            TokenType = type,
            Claims = new Dictionary<string, object> { ["sub"] = userId.ToString() },
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                algorithm),
        });
    }

    public static string Unsigned(Guid userId, string issuer, string audience, DateTimeOffset expiresAt)
    {
        string header = Encode("""{"alg":"none","typ":"at+jwt"}""");
        string payload = Payload(userId, issuer, audience, expiresAt);

        return $"{header}.{payload}.";
    }

    public static string WithForgedPayload(
        string token,
        Guid userId,
        string issuer,
        string audience,
        DateTimeOffset expiresAt)
    {
        string[] parts = token.Split('.');

        return $"{parts[0]}.{Payload(userId, issuer, audience, expiresAt)}.{parts[2]}";
    }

    private static string Payload(Guid userId, string issuer, string audience, DateTimeOffset expiresAt)
    {
        long expiry = expiresAt.ToUnixTimeSeconds();
        return Encode($$"""{"sub":"{{userId}}","iss":"{{issuer}}","aud":"{{audience}}","exp":{{expiry}}}""");
    }

    private static string Encode(string json)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes(json)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
