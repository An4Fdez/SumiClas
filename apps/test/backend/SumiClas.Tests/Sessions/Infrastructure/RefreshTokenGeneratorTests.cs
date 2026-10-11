using System.Text.RegularExpressions;
using SumiClas.Sessions.Infrastructure;

namespace SumiClas.Tests.Sessions.Infrastructure;

[TestFixture]
public class RefreshTokenGeneratorTests
{
    private readonly RefreshTokenGenerator _generator = new();

    [Test]
    public void Generate_ReturnsA43CharBase64UrlValueWithoutPadding()
    {
        var raw = _generator.Generate();

        Assert.That(raw, Has.Length.EqualTo(43));
        Assert.That(raw, Does.Match("^[A-Za-z0-9_-]+$"));
    }

    [Test]
    public void Generate_TwiceReturnsDifferentValues()
    {
        var first = _generator.Generate();
        var second = _generator.Generate();

        Assert.That(first, Is.Not.EqualTo(second));
        Assert.That(_generator.Hash(first), Is.Not.EqualTo(_generator.Hash(second)));
    }

    [Test]
    public void Hash_ReturnsA64CharLowercaseHexValueThatIsNotTheRawValue()
    {
        var raw = _generator.Generate();

        var hash = _generator.Hash(raw);

        Assert.That(hash, Has.Length.EqualTo(64));
        Assert.That(Regex.IsMatch(hash, "^[0-9a-f]{64}$"), Is.True);
        Assert.That(hash, Is.Not.EqualTo(raw));
    }

    [Test]
    public void Hash_IsDeterministicForTheSameValue()
    {
        var raw = _generator.Generate();

        Assert.That(_generator.Hash(raw), Is.EqualTo(_generator.Hash(raw)));
    }

    [Test]
    public void Hash_MatchesTheKnownSha256OfTheUtf8Bytes()
    {
        Assert.That(_generator.Hash("abc"), Is.EqualTo("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad"));
    }
}
