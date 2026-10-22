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
    public void create_the_token_with_a_value_and_an_expiry()
    {
        string value = StringMother.Random("jwt");
        DateTimeOffset expiresAt = UnitTestCase.Now + AccessToken.Lifetime;

        AccessToken result = AccessToken.Create(value, expiresAt);

        result.Value.Should().Be(value);
        result.ExpiresAt.Should().Be(expiresAt);
    }

    [TestCase(null)]
    [TestCase("   ")]
    public void throw_value_required_when_created_with_an_empty_value(string? value)
    {
        DateTimeOffset expiresAt = UnitTestCase.Now + AccessToken.Lifetime;

        Action act = () => AccessToken.Create(value!, expiresAt);


        act.Should().Throw<AccessTokenExceptions.ValueRequired>();
    }

    [Test]
    public void throw_expiry_required_when_created_with_a_default_expiry()
    {
        string value = StringMother.Random("jwt");

        Action act = () => AccessToken.Create(value, default);


        act.Should().Throw<AccessTokenExceptions.ExpiryRequired>();
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
