namespace SumiClas.Tests.Api;

using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SumiClas.Api.Configuration;
using SumiClas.Sessions.Infrastructure;
using SumiClas.Tests.Sessions.Infrastructure;
using SumiClas.Tests.Shared;

[TestFixture]
public class JwtOptionsValidationShould
{
    [Test]
    public void succeed_when_the_key_has_32_bytes()
    {
        string issuer = StringMother.Random("issuer");
        string audience = StringMother.Random("audience");
        JwtOptions expected = JwtOptionsMother.Create(issuer, audience, new string('k', 32));

        JwtOptions options = Resolve(JwtSettingsMother.From(expected)).Value;

        options.SigningKey.Should().HaveLength(32);
        options.Issuer.Should().Be(issuer);
        options.Audience.Should().Be(audience);
    }

    [Test]
    public void fail_with_a_message_that_says_how_to_define_it_when_the_signing_key_is_missing()
    {
        Func<JwtOptions> act = () => Resolve(JwtSettingsMother.WithoutSigningKey()).Value;

        act.Should().ThrowExactly<OptionsValidationException>().Which.Message.Should().Contain("Jwt__SigningKey");
    }

    [Test]
    public void fail_when_the_key_has_31_bytes()
    {
        Func<JwtOptions> act = () => Resolve(JwtSettingsMother.With(signingKey: new string('k', 31))).Value;

        act.Should().ThrowExactly<OptionsValidationException>().Which.Message.Should().Contain("32 bytes");
    }

    [Test]
    public void fail_when_the_issuer_is_missing()
    {
        Func<JwtOptions> act = () => Resolve(JwtSettingsMother.WithoutIssuer()).Value;

        act.Should().ThrowExactly<OptionsValidationException>().Which.Message.Should().Contain("Jwt:Issuer");
    }

    [Test]
    public void fail_when_the_audience_is_missing()
    {
        Func<JwtOptions> act = () => Resolve(JwtSettingsMother.WithoutAudience()).Value;

        act.Should().ThrowExactly<OptionsValidationException>().Which.Message.Should().Contain("Jwt:Audience");
    }

    private static IOptions<JwtOptions> Resolve(Dictionary<string, string?> values)
    {
        IConfiguration configuration = ConfigurationMother.Create(values);
        ServiceProvider provider = new ServiceCollection().AddSumiClasJwtOptions(configuration).BuildServiceProvider();
        return provider.GetRequiredService<IOptions<JwtOptions>>();
    }
}
