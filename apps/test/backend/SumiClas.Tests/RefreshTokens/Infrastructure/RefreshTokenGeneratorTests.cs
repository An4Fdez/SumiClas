using System.Text.RegularExpressions;
using SumiClas.RefreshTokens.Infrastructure;

namespace SumiClas.Tests.RefreshTokens.Infrastructure;

[TestFixture]
public class RefreshTokenGeneratorTests
{
    private readonly RefreshTokenGenerator _generator = new();

    [Test]
    public void Generate_ReturnsA43CharBase64UrlRawValueWithoutPadding()
    {
        var (raw, _) = _generator.Generate();

        Assert.That(raw, Has.Length.EqualTo(43));
        Assert.That(raw, Does.Match("^[A-Za-z0-9_-]+$"));
    }

    [Test]
    public void Generate_TwiceReturnsDifferentValues()
    {
        var first = _generator.Generate();
        var second = _generator.Generate();

        Assert.That(first.Raw, Is.Not.EqualTo(second.Raw));
        Assert.That(first.Hash, Is.Not.EqualTo(second.Hash));
    }

    [Test]
    public void Generate_ReturnsA64CharLowercaseHexHashThatIsNotTheRawValue()
    {
        var (raw, hash) = _generator.Generate();

        Assert.That(hash, Has.Length.EqualTo(64));
        Assert.That(Regex.IsMatch(hash, "^[0-9a-f]{64}$"), Is.True);
        Assert.That(hash, Is.Not.EqualTo(raw));
    }

    [Test]
    public void Hash_OfTheRawValueEqualsTheHashFromGenerate()
    {
        var (raw, hash) = _generator.Generate();

        Assert.That(_generator.Hash(raw), Is.EqualTo(hash));
    }

    [Test]
    public void Hash_MatchesTheKnownSha256OfTheUtf8Bytes()
    {
        Assert.That(_generator.Hash("abc"), Is.EqualTo("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad"));
    }
}
