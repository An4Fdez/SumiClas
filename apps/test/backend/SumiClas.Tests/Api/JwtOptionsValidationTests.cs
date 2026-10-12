using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SumiClas.Api.Configuration;
using SumiClas.Sessions.Infrastructure;

namespace SumiClas.Tests.Api;

[TestFixture]
public class JwtOptionsValidationTests
{
    private static IOptions<JwtOptions> Resolve(Dictionary<string, string?> values)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        var provider = new ServiceCollection().AddSumiClasJwtOptions(configuration).BuildServiceProvider();
        return provider.GetRequiredService<IOptions<JwtOptions>>();
    }

    private static Dictionary<string, string?> Valid() => new()
    {
        ["Jwt:Issuer"] = "sumiclas-api",
        ["Jwt:Audience"] = "sumiclas-web",
        ["Jwt:SigningKey"] = new string('k', 32),
    };

    [Test]
    public void Options_WithA32ByteKey_Succeed()
    {
        var options = Resolve(Valid()).Value;

        Assert.That(options.SigningKey, Has.Length.EqualTo(32));
        Assert.That(options.Issuer, Is.EqualTo("sumiclas-api"));
        Assert.That(options.Audience, Is.EqualTo("sumiclas-web"));
    }

    [Test]
    public void Options_WithoutTheSigningKey_FailWithAMessageThatSaysHowToDefineIt()
    {
        var values = Valid();
        values.Remove("Jwt:SigningKey");

        Assert.That(() => Resolve(values).Value,
            Throws.TypeOf<OptionsValidationException>().With.Message.Contains("Jwt__SigningKey"));
    }

    [Test]
    public void Options_WithA31ByteKey_Fail()
    {
        var values = Valid();
        values["Jwt:SigningKey"] = new string('k', 31);

        Assert.That(() => Resolve(values).Value,
            Throws.TypeOf<OptionsValidationException>().With.Message.Contains("32 bytes"));
    }

    [Test]
    public void Options_WithoutTheIssuer_Fail()
    {
        var values = Valid();
        values.Remove("Jwt:Issuer");

        Assert.That(() => Resolve(values).Value,
            Throws.TypeOf<OptionsValidationException>().With.Message.Contains("Jwt:Issuer"));
    }

    [Test]
    public void Options_WithoutTheAudience_Fail()
    {
        var values = Valid();
        values.Remove("Jwt:Audience");

        Assert.That(() => Resolve(values).Value,
            Throws.TypeOf<OptionsValidationException>().With.Message.Contains("Jwt:Audience"));
    }
}
