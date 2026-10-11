using SumiClas.RefreshTokens.Domain;

namespace SumiClas.Tests.RefreshTokens.Domain;

[TestFixture]
public class AccessTokenTests
{
    private static readonly DateTimeOffset ExpiresAt = new(2026, 10, 11, 12, 15, 0, TimeSpan.Zero);

    [Test]
    public void Create_WithValueAndExpiry_Succeeds()
    {
        var result = AccessToken.Create("jwt", ExpiresAt);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Value, Is.EqualTo("jwt"));
        Assert.That(result.Value.ExpiresAt, Is.EqualTo(ExpiresAt));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Create_WithEmptyValue_FailsWithValueRequired(string? value)
    {
        var result = AccessToken.Create(value!, ExpiresAt);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(AccessTokenErrors.ValueRequired));
    }

    [Test]
    public void Create_WithDefaultExpiry_FailsWithExpiryRequired()
    {
        var result = AccessToken.Create("jwt", default);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(AccessTokenErrors.ExpiryRequired));
    }

    [Test]
    public void Equality_IsByValueAndExpiry()
    {
        var first = AccessToken.Create("jwt", ExpiresAt).Value;
        var same = AccessToken.Create("jwt", ExpiresAt).Value;
        var otherValue = AccessToken.Create("other", ExpiresAt).Value;
        var otherExpiry = AccessToken.Create("jwt", ExpiresAt.AddMinutes(1)).Value;

        Assert.That(first, Is.EqualTo(same));
        Assert.That(first, Is.Not.EqualTo(otherValue));
        Assert.That(first, Is.Not.EqualTo(otherExpiry));
    }
}
