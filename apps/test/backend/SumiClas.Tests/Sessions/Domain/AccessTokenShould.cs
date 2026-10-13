namespace SumiClas.Tests.Sessions.Domain;

using AwesomeAssertions;
using SumiClas.Sessions.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class AccessTokenShould
{

    [Test]
    public void have_a_lifetime_of_15_minutes()
    {
        AccessToken.Lifetime.Should().Be(TimeSpan.FromMinutes(15));
    }

    [Test]
    public void succeed_when_created_with_a_value_and_an_expiry()
    {
        string value = StringMother.Random("jwt");
        DateTimeOffset expiresAt = UnitTestCase.Now + AccessToken.Lifetime;

        Result<AccessToken> result = AccessToken.Create(value, expiresAt);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(value);
        result.Value.ExpiresAt.Should().Be(expiresAt);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void fail_with_value_required_when_created_with_an_empty_value(string? value)
    {
        DateTimeOffset expiresAt = UnitTestCase.Now + AccessToken.Lifetime;

        Result<AccessToken> result = AccessToken.Create(value!, expiresAt);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccessTokenErrors.ValueRequired);
    }

    [Test]
    public void fail_with_expiry_required_when_created_with_a_default_expiry()
    {
        string value = StringMother.Random("jwt");

        Result<AccessToken> result = AccessToken.Create(value, default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccessTokenErrors.ExpiryRequired);
    }

    [Test]
    public void be_equal_only_when_value_and_expiry_are_equal()
    {
        string value = StringMother.Random("jwt");
        DateTimeOffset expiresAt = UnitTestCase.Now + AccessToken.Lifetime;
        AccessToken first = AccessTokenMother.Create(value, expiresAt);
        AccessToken same = AccessTokenMother.Create(value, expiresAt);
        AccessToken otherValue = AccessTokenMother.Create(StringMother.Random("other"), expiresAt);
        AccessToken otherExpiry = AccessTokenMother.Create(value, expiresAt.AddMinutes(1));

        first.Should().Be(same);
        first.Should().NotBe(otherValue);
        first.Should().NotBe(otherExpiry);
    }
}
