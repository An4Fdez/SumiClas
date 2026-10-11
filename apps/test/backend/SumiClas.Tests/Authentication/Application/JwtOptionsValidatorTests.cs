using SumiClas.Authentication.Application;

namespace SumiClas.Tests.Authentication.Application;

[TestFixture]
public class JwtOptionsValidatorTests
{
    private readonly JwtOptionsValidator _validator = new();

    private static JwtOptions Valid() => new()
    {
        Issuer = "sumiclas-api",
        Audience = "sumiclas-web",
        SigningKey = new string('k', 32),
        AccessTokenMinutes = 15,
        RefreshTokenDays = 14,
        RefreshGraceSeconds = 10,
    };

    [Test]
    public void Validate_WithValidOptions_Succeeds()
    {
        Assert.That(_validator.Validate(null, Valid()).Succeeded, Is.True);
    }

    [Test]
    public void Validate_WithAShortKey_Fails()
    {
        var options = Valid();
        options.SigningKey = new string('k', 16);

        var result = _validator.Validate(null, options);

        Assert.That(result.Failed, Is.True);
        Assert.That(result.FailureMessage, Does.Contain("al menos 32"));
    }

    [Test]
    public void Validate_CountsBytesNotCharacters()
    {
        var options = Valid();
        options.SigningKey = new string('ñ', 16);

        Assert.That(_validator.Validate(null, options).Succeeded, Is.True);
    }

    [TestCase("")]
    [TestCase("   ")]
    public void Validate_WithoutAKey_ExplainsHowToDefineIt(string key)
    {
        var options = Valid();
        options.SigningKey = key;

        var result = _validator.Validate(null, options);

        Assert.That(result.Failed, Is.True);
        Assert.That(result.FailureMessage, Does.Contain("dotnet user-secrets set \"Jwt:SigningKey\""));
        Assert.That(result.FailureMessage, Does.Contain("Jwt__SigningKey"));
    }

    [Test]
    public void Validate_WithoutIssuerOrAudience_Fails()
    {
        var options = Valid();
        options.Issuer = "";
        options.Audience = " ";

        var result = _validator.Validate(null, options);

        Assert.That(result.Failures, Has.Some.Contain("Jwt:Issuer"));
        Assert.That(result.Failures, Has.Some.Contain("Jwt:Audience"));
    }

    [TestCase(0, false)]
    [TestCase(1, true)]
    [TestCase(60, true)]
    [TestCase(61, false)]
    public void Validate_AccessTokenMinutesRange(int minutes, bool valid)
    {
        var options = Valid();
        options.AccessTokenMinutes = minutes;

        Assert.That(_validator.Validate(null, options).Succeeded, Is.EqualTo(valid));
    }

    [TestCase(0, false)]
    [TestCase(1, true)]
    [TestCase(90, true)]
    [TestCase(91, false)]
    public void Validate_RefreshTokenDaysRange(int days, bool valid)
    {
        var options = Valid();
        options.RefreshTokenDays = days;

        Assert.That(_validator.Validate(null, options).Succeeded, Is.EqualTo(valid));
    }

    [TestCase(-1, false)]
    [TestCase(0, true)]
    [TestCase(60, true)]
    [TestCase(61, false)]
    public void Validate_RefreshGraceSecondsRange(int seconds, bool valid)
    {
        var options = Valid();
        options.RefreshGraceSeconds = seconds;

        Assert.That(_validator.Validate(null, options).Succeeded, Is.EqualTo(valid));
    }
}
