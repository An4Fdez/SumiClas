using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Moq;
using SumiClas.Authentication.Application;
using SumiClas.Authentication.Infrastructure;
using SumiClas.Shared.Domain;

namespace SumiClas.Tests.Authentication.Infrastructure;

[TestFixture]
public class JwtAccessTokenIssuerTests
{
    private const string Key = "unit-test-signing-key-with-at-least-32-bytes!";
    private static readonly DateTimeOffset Now = new(2026, 3, 1, 10, 0, 0, TimeSpan.Zero);

    private readonly Guid _userId = Guid.NewGuid();
    private JwtOptions _options = null!;
    private JwtAccessTokenIssuer _issuer = null!;

    [SetUp]
    public void SetUp()
    {
        _options = new JwtOptions
        {
            Issuer = "sumiclas-api",
            Audience = "sumiclas-web",
            SigningKey = Key,
            AccessTokenMinutes = 15,
            RefreshTokenDays = 14,
            RefreshGraceSeconds = 10,
        };
        var clock = new Mock<IClock>();
        clock.Setup(c => c.UtcNow).Returns(Now);
        _issuer = new JwtAccessTokenIssuer(Options.Create(_options), clock.Object);
    }

    [Test]
    public void Issue_ProducesAnAtJwtWithTheExpectedClaims()
    {
        var token = new JsonWebToken(_issuer.Issue(_userId).Value);

        Assert.That(token.Typ, Is.EqualTo("at+jwt"));
        Assert.That(token.Alg, Is.EqualTo("HS256"));
        Assert.That(token.Subject, Is.EqualTo(_userId.ToString()));
        Assert.That(Guid.TryParse(token.Id, out _), Is.True);
        Assert.That(token.Issuer, Is.EqualTo("sumiclas-api"));
        Assert.That(token.Audiences, Is.EqualTo(new[] { "sumiclas-web" }));
        Assert.That(token.IssuedAt, Is.EqualTo(Now.UtcDateTime));
        Assert.That(token.ValidFrom, Is.EqualTo(Now.UtcDateTime));
        Assert.That(token.ValidTo, Is.EqualTo(Now.AddMinutes(15).UtcDateTime));
    }

    [Test]
    public void Issue_ReturnsTheExpiryFromTheClockAndTheConfiguredLifetime()
    {
        Assert.That(_issuer.Issue(_userId).ExpiresAt, Is.EqualTo(Now.AddMinutes(15)));
    }

    [Test]
    public void Issue_UsesAFreshJtiOnEveryCall()
    {
        var first = new JsonWebToken(_issuer.Issue(_userId).Value);
        var second = new JsonWebToken(_issuer.Issue(_userId).Value);

        Assert.That(first.Id, Is.Not.EqualTo(second.Id));
    }

    [Test]
    public void Issue_DoesNotIncludePersonalData()
    {
        var token = new JsonWebToken(_issuer.Issue(_userId).Value);

        var names = token.Claims.Select(claim => claim.Type).ToList();

        Assert.That(names, Is.SupersetOf(new[] { "sub", "jti", "iat", "nbf", "exp", "iss", "aud" }));
        Assert.That(names, Has.None.EqualTo("email"));
        Assert.That(names, Has.None.EqualTo("name"));
        Assert.That(names, Has.None.EqualTo("given_name"));
    }

    [Test]
    public async Task Issue_SignatureValidatesWithTheKeyAndFailsWithAnotherOne()
    {
        var value = _issuer.Issue(_userId).Value;
        var handler = new JsonWebTokenHandler();

        var withKey = await handler.ValidateTokenAsync(value, Parameters(Key));
        var withOtherKey = await handler.ValidateTokenAsync(value, Parameters("another-signing-key-with-at-least-32-bytes!"));

        Assert.That(withKey.IsValid, Is.True);
        Assert.That(withOtherKey.IsValid, Is.False);
    }

    private TokenValidationParameters Parameters(string key) => new()
    {
        ValidIssuer = _options.Issuer,
        ValidAudience = _options.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidateLifetime = false,
    };
}
