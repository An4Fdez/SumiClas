using SumiClas.Shared.Domain;

namespace SumiClas.Tests.Shared.Domain;

[TestFixture]
public class TextNormalizerTests
{
    [TestCase("  CEIP   Sumi  ", "ceip sumi")]
    [TestCase("Málaga", "malaga")]
    [TestCase("Peña", "pena")]
    [TestCase("Ñ", "n")]
    [TestCase("Santa Cruz de Tenerife", "santa cruz de tenerife")]
    [TestCase("", "")]
    [TestCase("   ", "")]
    [TestCase(null, "")]
    public void Normalize_ReturnsTrimmedLowercaseTextWithoutDiacritics(string? text, string expected)
    {
        Assert.That(TextNormalizer.Normalize(text), Is.EqualTo(expected));
    }

    [Test]
    public void Normalize_IsIdempotent()
    {
        var once = TextNormalizer.Normalize("  Álava  /  Araba ");

        Assert.That(TextNormalizer.Normalize(once), Is.EqualTo(once));
    }
}
