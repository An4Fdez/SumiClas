using SumiClas.Shared.Domain;
using SumiClas.Users.Domain;

namespace SumiClas.Tests.Users.Domain;

[TestFixture]
public class PasswordTests
{
    [TestCase("12345678", true)]
    [TestCase("1234567", false)]
    [TestCase("", false)]
    [TestCase(null, false)]
    public void Create_EnforcesTheMinimumLength(string? value, bool valid)
    {
        var result = Password.Create(value);

        Assert.That(result.IsSuccess, Is.EqualTo(valid));
        if (!valid) Assert.That(result.Error, Is.EqualTo(AuthenticationErrors.PasswordTooWeak));
    }

    [Test]
    public void Create_RejectsMoreThanTheMaximumLength()
    {
        Assert.That(Password.Create(new string('a', Password.MaxLength)).IsSuccess, Is.True);
        Assert.That(Password.Create(new string('a', Password.MaxLength + 1)).Error, Is.EqualTo(AuthenticationErrors.PasswordTooWeak));
    }

    [Test]
    public void Create_KeepsSpacesAsTyped()
    {
        var result = Password.Create("  spaced out  ");

        Assert.That(result.Value.Value, Is.EqualTo("  spaced out  "));
    }

    [Test]
    public void Create_DoesNotTrimToSatisfyTheMinimum()
    {
        Assert.That(Password.Create("  abc  ").IsSuccess, Is.False);
        Assert.That(Password.Create("1234567 ").IsSuccess, Is.True);
        Assert.That(Password.Create("        ").IsSuccess, Is.True);
    }

    [Test]
    public void Equality_IsByValue()
    {
        var first = Password.Create("same-password").Value;
        var second = Password.Create("same-password").Value;
        var other = Password.Create("other-password").Value;

        Assert.That(first, Is.EqualTo(second));
        Assert.That(first, Is.Not.EqualTo(other));
    }

    [Test]
    public void ToString_NeverContainsTheValue()
    {
        var password = Password.Create("super-secret-value").Value;

        Assert.That(password.ToString(), Does.Not.Contain("super-secret-value"));
        Assert.That($"{password}", Does.Not.Contain("super-secret"));
    }
}
