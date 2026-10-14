namespace SumiClas.Tests.Api;

using AwesomeAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SumiClas.Api.Configuration;
using SumiClas.Sessions.Domain;
using SumiClas.Sessions.Infrastructure;
using SumiClas.Tests.Sessions.Infrastructure;
using SumiClas.Tests.Shared;
using System.Text.Json;

[TestFixture]
public class JwtBearerValidationShould : ApiUnitTestCase
{
    [Test]
    public async Task accept_a_valid_token_and_keep_the_sub_claim_name()
    {
        Guid userId = GuidMother.Random();
        JwtOptions options = JwtOptionsMother.Random();
        JwtBearerOptions bearer = BearerFor(options);
        this.ShouldHaveClockAt(DateTimeOffset.UtcNow);

        TokenValidationResult result = await ValidateAsync(bearer, this.Issue(userId, options));

        result.IsValid.Should().BeTrue(result.Exception?.Message);
        result.ClaimsIdentity.FindFirst("sub")?.Value.Should().Be(userId.ToString());
        bearer.MapInboundClaims.Should().BeFalse();
    }

    [Test]
    public async Task reject_a_tampered_signature()
    {
        Guid userId = GuidMother.Random();
        JwtOptions options = JwtOptionsMother.Random();
        JwtBearerOptions bearer = BearerFor(options);
        this.ShouldHaveClockAt(DateTimeOffset.UtcNow);
        string[] parts = this.Issue(userId, options).Split('.');
        char[] signature = parts[2].ToCharArray();
        signature[10] = signature[10] == 'A' ? 'B' : 'A';

        TokenValidationResult result = await ValidateAsync(
            bearer,
            $"{parts[0]}.{parts[1]}.{new string(signature)}");

        result.IsValid.Should().BeFalse();
    }

    [Test]
    public async Task reject_a_tampered_payload()
    {
        Guid userId = GuidMother.Random();
        JwtOptions options = JwtOptionsMother.Random();
        JwtBearerOptions bearer = BearerFor(options);
        this.ShouldHaveClockAt(DateTimeOffset.UtcNow);
        string token = JwtTokenMother.WithForgedPayload(
            this.Issue(userId, options),
            GuidMother.Random(),
            options.Issuer,
            options.Audience,
            DateTimeOffset.UtcNow.AddMinutes(5));

        TokenValidationResult result = await ValidateAsync(bearer, token);

        result.IsValid.Should().BeFalse();
    }

    [Test]
    public async Task reject_a_token_signed_with_another_key()
    {
        Guid userId = GuidMother.Random();
        JwtOptions options = JwtOptionsMother.Random();
        JwtBearerOptions bearer = BearerFor(options);
        this.ShouldHaveClockAt(DateTimeOffset.UtcNow);

        TokenValidationResult result = await ValidateAsync(
            bearer,
            this.Issue(userId, options, key: new string('z', 64)));

        result.IsValid.Should().BeFalse();
    }

    [Test]
    public async Task reject_a_wrong_issuer()
    {
        Guid userId = GuidMother.Random();
        JwtOptions options = JwtOptionsMother.Random();
        JwtBearerOptions bearer = BearerFor(options);
        this.ShouldHaveClockAt(DateTimeOffset.UtcNow);

        TokenValidationResult result = await ValidateAsync(
            bearer,
            this.Issue(userId, options, issuer: StringMother.Random("issuer")));

        result.IsValid.Should().BeFalse();
    }

    [Test]
    public async Task reject_a_wrong_audience()
    {
        Guid userId = GuidMother.Random();
        JwtOptions options = JwtOptionsMother.Random();
        JwtBearerOptions bearer = BearerFor(options);
        this.ShouldHaveClockAt(DateTimeOffset.UtcNow);

        TokenValidationResult result = await ValidateAsync(
            bearer,
            this.Issue(userId, options, audience: StringMother.Random("audience")));

        result.IsValid.Should().BeFalse();
    }

    [Test]
    public async Task reject_a_token_expired_beyond_the_clock_skew()
    {
        Guid userId = GuidMother.Random();
        JwtOptions options = JwtOptionsMother.Random();
        JwtBearerOptions bearer = BearerFor(options);
        this.ShouldHaveClockAt(DateTimeOffset.UtcNow - AccessToken.Lifetime - TimeSpan.FromMinutes(5));
        string token = this.Issue(userId, options);

        TokenValidationResult result = await ValidateAsync(bearer, token);

        result.IsValid.Should().BeFalse();
    }

    [Test]
    public async Task accept_a_token_expired_within_the_clock_skew()
    {
        Guid userId = GuidMother.Random();
        JwtOptions options = JwtOptionsMother.Random();
        JwtBearerOptions bearer = BearerFor(options);
        this.ShouldHaveClockAt(DateTimeOffset.UtcNow - AccessToken.Lifetime - TimeSpan.FromSeconds(10));
        string token = this.Issue(userId, options);

        bearer.TokenValidationParameters.ClockSkew.Should().Be(TimeSpan.FromSeconds(30));
        (await ValidateAsync(bearer, token)).IsValid.Should().BeTrue();
    }

    [Test]
    public async Task reject_a_wrong_token_type()
    {
        Guid userId = GuidMother.Random();
        JwtOptions options = JwtOptionsMother.Random();
        JwtBearerOptions bearer = BearerFor(options);

        TokenValidationResult result = await ValidateAsync(
            bearer,
            Descriptor(userId, options, SecurityAlgorithms.HmacSha256, "JWT"));

        result.IsValid.Should().BeFalse();
    }

    [Test]
    public async Task accept_the_expected_token_type_from_the_same_descriptor()
    {
        Guid userId = GuidMother.Random();
        JwtOptions options = JwtOptionsMother.Random();
        JwtBearerOptions bearer = BearerFor(options);

        TokenValidationResult result = await ValidateAsync(
            bearer,
            Descriptor(userId, options, SecurityAlgorithms.HmacSha256, "at+jwt"));

        result.IsValid.Should().BeTrue();
    }

    [Test]
    public async Task reject_an_hs512_token()
    {
        Guid userId = GuidMother.Random();
        JwtOptions options = JwtOptionsMother.Random();
        JwtBearerOptions bearer = BearerFor(options);

        TokenValidationResult result = await ValidateAsync(
            bearer,
            Descriptor(userId, options, SecurityAlgorithms.HmacSha512, "at+jwt"));

        result.IsValid.Should().BeFalse();
    }

    [Test]
    public async Task reject_an_unsigned_alg_none_token()
    {
        Guid userId = GuidMother.Random();
        JwtOptions options = JwtOptionsMother.Random();
        JwtBearerOptions bearer = BearerFor(options);
        string token = JwtTokenMother.Unsigned(
            userId,
            options.Issuer,
            options.Audience,
            DateTimeOffset.UtcNow.AddMinutes(5));

        TokenValidationResult result = await ValidateAsync(bearer, token);

        result.IsValid.Should().BeFalse();
    }

    [Test]
    public void require_signed_tokens_with_the_configured_lifetime_issuer_and_audience()
    {
        JwtBearerOptions bearer = BearerFor(JwtOptionsMother.Random());

        TokenValidationParameters parameters = bearer.TokenValidationParameters;

        parameters.RequireSignedTokens.Should().BeTrue();
        parameters.ValidateLifetime.Should().BeTrue();
        parameters.ValidateIssuer.Should().BeTrue();
        parameters.ValidateAudience.Should().BeTrue();
        parameters.ValidateIssuerSigningKey.Should().BeTrue();
        parameters.ValidAlgorithms.Should().Equal(SecurityAlgorithms.HmacSha256);
        parameters.ValidTypes.Should().Equal("at+jwt");
    }

    [Test]
    public async Task answer_an_unauthenticated_problem_and_handle_the_response_when_challenged()
    {
        JwtBearerOptions bearer = BearerFor(JwtOptionsMother.Random());
        DefaultHttpContext context = HttpContextMother.WithProblemDetails();
        JwtBearerChallengeContext challenge = new(
            context,
            BearerScheme(),
            bearer,
            new AuthenticationProperties());

        await bearer.Events.Challenge(challenge);

        challenge.Handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(401);
        JsonElement body = await ReadBodyAsync(context);
        body.GetProperty("code").GetString().Should().Be("Authentication.Unauthenticated");
        body.GetProperty("status").GetInt32().Should().Be(401);
        body.TryGetProperty("type", out _).Should().BeTrue();
        body.TryGetProperty("title", out _).Should().BeTrue();
        context.Response.Headers.WWWAuthenticate.ToString().Should().Be("Bearer");
    }

    [Test]
    public async Task answer_a_forbidden_problem_when_forbidden()
    {
        JwtBearerOptions bearer = BearerFor(JwtOptionsMother.Random());
        DefaultHttpContext context = HttpContextMother.WithProblemDetails();
        ForbiddenContext forbidden = new(context, BearerScheme(), bearer);

        await bearer.Events.Forbidden(forbidden);

        context.Response.StatusCode.Should().Be(403);
        JsonElement body = await ReadBodyAsync(context);
        body.GetProperty("code").GetString().Should().Be("Authentication.Forbidden");
        body.GetProperty("status").GetInt32().Should().Be(403);
    }

    private static JwtBearerOptions BearerFor(JwtOptions options)
    {
        IConfiguration configuration = ConfigurationMother.Create(JwtSettingsMother.From(options));
        ServiceProvider provider = new ServiceCollection()
            .AddLogging()
            .AddSumiClasJwtOptions(configuration)
            .AddSumiClasApiSecurity(configuration)
            .BuildServiceProvider();
        return provider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);
    }

    private static string Descriptor(Guid userId, JwtOptions options, string algorithm, string type)
        => JwtTokenMother.Create(
            userId,
            options.Issuer,
            options.Audience,
            options.SigningKey,
            algorithm,
            type,
            DateTimeOffset.UtcNow);

    private static async Task<TokenValidationResult> ValidateAsync(JwtBearerOptions bearer, string token)
        => await new JsonWebTokenHandler().ValidateTokenAsync(token, bearer.TokenValidationParameters);

    private static AuthenticationScheme BearerScheme() => new("Bearer", null, typeof(JwtBearerHandler));

    private void ShouldHaveClockAt(DateTimeOffset at)
        => this.Clock.Setup(clock => clock.UtcNow).Returns(at);

    private string Issue(
        Guid userId,
        JwtOptions options,
        string? issuer = null,
        string? audience = null,
        string? key = null)
    {
        JwtOptions issuing = JwtOptionsMother.With(
            issuer ?? options.Issuer,
            audience ?? options.Audience,
            key ?? options.SigningKey);

        return new JwtAccessTokenGenerator(Options.Create(issuing), this.Clock.Object)
            .Generate(userId)
            .Value
            .Value;
    }
}
