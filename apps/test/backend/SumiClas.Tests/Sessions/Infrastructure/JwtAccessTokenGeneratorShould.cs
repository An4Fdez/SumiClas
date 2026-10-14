namespace SumiClas.Tests.Sessions.Infrastructure;

using AwesomeAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SumiClas.Sessions.Domain;
using SumiClas.Sessions.Infrastructure;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Sessions.Domain;
using SumiClas.Tests.Shared;
using System.Text;

[TestFixture]
public class JwtAccessTokenGeneratorShould : UnitTestCase
{
    [Test]
    public void produce_an_at_jwt_with_the_expected_claims()
    {
        Guid userId = GuidMother.Random();
        JwtOptions options = JwtOptionsMother.Random();

        JsonWebToken token = GenerateToken(this.NewGenerator(options), userId);

        token.Typ.Should().Be("at+jwt");
        token.Alg.Should().Be("HS256");
        token.Subject.Should().Be(userId.ToString());
        Guid.TryParse(token.Id, out _).Should().BeTrue();
        token.Issuer.Should().Be(options.Issuer);
        token.Audiences.Should().Equal(options.Audience);
        token.IssuedAt.Should().Be(Now.UtcDateTime);
        token.ValidFrom.Should().Be(Now.UtcDateTime);
        token.ValidTo.Should().Be((Now + AccessToken.Lifetime).UtcDateTime);
    }

    [Test]
    public void return_a_successful_result_with_an_access_token_value_object()
    {
        JwtAccessTokenGenerator generator = this.NewGenerator(JwtOptionsMother.Random());

        Result<AccessToken> result = generator.Generate(GuidMother.Random());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(AccessTokenMother.Create(result.Value.Value, Now + AccessToken.Lifetime));
    }

    [Test]
    public void use_a_fresh_jti_on_every_call()
    {
        Guid userId = GuidMother.Random();
        JwtAccessTokenGenerator generator = this.NewGenerator(JwtOptionsMother.Random());

        JsonWebToken first = GenerateToken(generator, userId);
        JsonWebToken second = GenerateToken(generator, userId);

        first.Id.Should().NotBe(second.Id);
    }

    [Test]
    public void not_include_personal_data()
    {
        JwtAccessTokenGenerator generator = this.NewGenerator(JwtOptionsMother.Random());
        JsonWebToken token = GenerateToken(generator, GuidMother.Random());

        List<string> names = token.Claims.Select(claim => claim.Type).ToList();

        names.Should().Contain(["sub", "jti", "iat", "nbf", "exp", "iss", "aud"]);
        names.Should().NotContain("email");
        names.Should().NotContain("name");
        names.Should().NotContain("given_name");
    }

    [Test]
    public async Task produce_a_signature_that_validates_with_the_key_and_fails_with_another_one()
    {
        JwtOptions options = JwtOptionsMother.Random();
        string value = this.NewGenerator(options).Generate(GuidMother.Random()).Value.Value;
        JsonWebTokenHandler handler = new();

        TokenValidationResult withKey = await handler.ValidateTokenAsync(
            value,
            Parameters(options, options.SigningKey));
        TokenValidationResult withOtherKey = await handler.ValidateTokenAsync(
            value,
            Parameters(options, StringMother.Random("another-signing-key") + "-with-at-least-32-bytes"));

        withKey.IsValid.Should().BeTrue();
        withOtherKey.IsValid.Should().BeFalse();
    }

    private static JsonWebToken GenerateToken(JwtAccessTokenGenerator generator, Guid userId)
        => new(generator.Generate(userId).Value.Value);

    private static TokenValidationParameters Parameters(JwtOptions options, string key)
        => new()
        {
            ValidIssuer = options.Issuer,
            ValidAudience = options.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidateLifetime = false,
        };

    private JwtAccessTokenGenerator NewGenerator(JwtOptions options)
        => new(Options.Create(options), this.Clock.Object);
}
