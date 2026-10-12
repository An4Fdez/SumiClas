using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Moq;
using SumiClas.Api.Configuration;
using SumiClas.Sessions.Domain;
using SumiClas.Sessions.Infrastructure;
using SumiClas.Shared.Domain;

namespace SumiClas.Tests.Api;

[TestFixture]
public class JwtBearerValidationTests
{
    private const string Key = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";
    private const string Issuer = "sumiclas-api";
    private const string Audience = "sumiclas-web";
    private static readonly Guid UserId = Guid.Parse("3f2b1c7e-8d4a-4c1e-9b5f-0a6d2e7c9b10");

    private JwtBearerOptions _bearer = null!;
    private DateTimeOffset _issuedAt;

    [SetUp]
    public void SetUp()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Issuer"] = Issuer,
            ["Jwt:Audience"] = Audience,
            ["Jwt:SigningKey"] = Key,
        }).Build();
        var provider = new ServiceCollection()
            .AddLogging()
            .AddSumiClasJwtOptions(configuration)
            .AddSumiClasApiSecurity(configuration)
            .BuildServiceProvider();
        _bearer = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(JwtBearerDefaults.AuthenticationScheme);
        _issuedAt = DateTimeOffset.UtcNow;
    }

    private string Issue(string issuer = Issuer, string audience = Audience, string key = Key, DateTimeOffset? at = null)
    {
        var clock = new Mock<IClock>();
        clock.Setup(c => c.UtcNow).Returns(at ?? _issuedAt);
        var options = Options.Create(new JwtOptions { Issuer = issuer, Audience = audience, SigningKey = key });
        return new JwtAccessTokenGenerator(options, clock.Object).Generate(UserId).Value.Value;
    }

    private async Task<TokenValidationResult> ValidateAsync(string token)
        => await new JsonWebTokenHandler().ValidateTokenAsync(token, _bearer.TokenValidationParameters);

    private static string Descriptor(string algorithm, string type, string? key = Key, DateTimeOffset? at = null)
    {
        var now = (at ?? DateTimeOffset.UtcNow).UtcDateTime;
        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = now.AddMinutes(15),
            TokenType = type,
            Claims = new Dictionary<string, object> { ["sub"] = UserId.ToString() },
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!)), algorithm),
        });
    }

    [Test]
    public async Task ValidToken_IsAccepted_AndKeepsTheSubClaimName()
    {
        var result = await ValidateAsync(Issue());

        Assert.That(result.IsValid, Is.True, result.Exception?.Message);
        Assert.That(result.ClaimsIdentity.FindFirst("sub")?.Value, Is.EqualTo(UserId.ToString()));
        Assert.That(_bearer.MapInboundClaims, Is.False);
    }

    [Test]
    public async Task TamperedSignature_IsRejected()
    {
        var token = Issue();
        var parts = token.Split('.');
        var signature = parts[2].ToCharArray();
        signature[10] = signature[10] == 'A' ? 'B' : 'A';

        var result = await ValidateAsync($"{parts[0]}.{parts[1]}.{new string(signature)}");

        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public async Task TamperedPayload_IsRejected()
    {
        var parts = Issue().Split('.');
        var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(
            $$"""{"sub":"{{Guid.NewGuid()}}","iss":"{{Issuer}}","aud":"{{Audience}}","exp":{{DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds()}}}""")).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        var result = await ValidateAsync($"{parts[0]}.{payload}.{parts[2]}");

        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public async Task TokenSignedWithAnotherKey_IsRejected()
    {
        var result = await ValidateAsync(Issue(key: new string('z', 64)));

        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public async Task WrongIssuer_IsRejected()
    {
        Assert.That((await ValidateAsync(Issue(issuer: "someone-else"))).IsValid, Is.False);
    }

    [Test]
    public async Task WrongAudience_IsRejected()
    {
        Assert.That((await ValidateAsync(Issue(audience: "another-app"))).IsValid, Is.False);
    }

    [Test]
    public async Task TokenExpiredBeyondTheClockSkew_IsRejected()
    {
        var token = Issue(at: DateTimeOffset.UtcNow - AccessToken.Lifetime - TimeSpan.FromMinutes(5));

        Assert.That((await ValidateAsync(token)).IsValid, Is.False);
    }

    [Test]
    public async Task TokenExpiredWithinTheClockSkew_IsAccepted()
    {
        var token = Issue(at: DateTimeOffset.UtcNow - AccessToken.Lifetime - TimeSpan.FromSeconds(10));

        Assert.That(_bearer.TokenValidationParameters.ClockSkew, Is.EqualTo(TimeSpan.FromSeconds(30)));
        Assert.That((await ValidateAsync(token)).IsValid, Is.True);
    }

    [Test]
    public async Task WrongTokenType_IsRejected()
    {
        Assert.That((await ValidateAsync(Descriptor(SecurityAlgorithms.HmacSha256, "JWT"))).IsValid, Is.False);
    }

    [Test]
    public async Task ExpectedTokenType_FromTheSameDescriptor_IsAccepted()
    {
        Assert.That((await ValidateAsync(Descriptor(SecurityAlgorithms.HmacSha256, "at+jwt"))).IsValid, Is.True);
    }

    [Test]
    public async Task Hs512Token_IsRejected()
    {
        Assert.That((await ValidateAsync(Descriptor(SecurityAlgorithms.HmacSha512, "at+jwt"))).IsValid, Is.False);
    }

    [Test]
    public async Task UnsignedAlgNoneToken_IsRejected()
    {
        static string Encode(string json) => Convert.ToBase64String(Encoding.UTF8.GetBytes(json)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var header = Encode("""{"alg":"none","typ":"at+jwt"}""");
        var payload = Encode($$"""{"sub":"{{UserId}}","iss":"{{Issuer}}","aud":"{{Audience}}","exp":{{DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds()}}}""");

        Assert.That((await ValidateAsync($"{header}.{payload}.")).IsValid, Is.False);
    }

    [Test]
    public void Parameters_RequireSignedTokensWithTheConfiguredLifetimeIssuerAndAudience()
    {
        var parameters = _bearer.TokenValidationParameters;

        Assert.That(parameters.RequireSignedTokens, Is.True);
        Assert.That(parameters.ValidateLifetime, Is.True);
        Assert.That(parameters.ValidateIssuer, Is.True);
        Assert.That(parameters.ValidateAudience, Is.True);
        Assert.That(parameters.ValidateIssuerSigningKey, Is.True);
        Assert.That(parameters.ValidAlgorithms, Is.EqualTo(new[] { SecurityAlgorithms.HmacSha256 }));
        Assert.That(parameters.ValidTypes, Is.EqualTo(new[] { "at+jwt" }));
    }

    [Test]
    public async Task OnChallenge_AnswersAnUnauthenticatedProblemAndHandlesTheResponse()
    {
        var context = ApiTestHelpers.NewHttpContext();
        var challenge = new JwtBearerChallengeContext(
            context, new AuthenticationScheme("Bearer", null, typeof(JwtBearerHandler)), _bearer, new AuthenticationProperties());

        await _bearer.Events.Challenge(challenge);

        Assert.That(challenge.Handled, Is.True);
        Assert.That(context.Response.StatusCode, Is.EqualTo(401));
        var body = await ApiTestHelpers.ReadBodyAsync(context);
        Assert.That(body.GetProperty("code").GetString(), Is.EqualTo("Authentication.Unauthenticated"));
        Assert.That(body.GetProperty("status").GetInt32(), Is.EqualTo(401));
        Assert.That(body.TryGetProperty("type", out _), Is.True);
        Assert.That(body.TryGetProperty("title", out _), Is.True);
        Assert.That(context.Response.Headers.WWWAuthenticate.ToString(), Is.EqualTo("Bearer"));
    }

    [Test]
    public async Task OnForbidden_AnswersAForbiddenProblem()
    {
        var context = ApiTestHelpers.NewHttpContext();
        var forbidden = new ForbiddenContext(
            context, new AuthenticationScheme("Bearer", null, typeof(JwtBearerHandler)), _bearer);

        await _bearer.Events.Forbidden(forbidden);

        Assert.That(context.Response.StatusCode, Is.EqualTo(403));
        var body = await ApiTestHelpers.ReadBodyAsync(context);
        Assert.That(body.GetProperty("code").GetString(), Is.EqualTo("Authentication.Forbidden"));
        Assert.That(body.GetProperty("status").GetInt32(), Is.EqualTo(403));
    }
}
