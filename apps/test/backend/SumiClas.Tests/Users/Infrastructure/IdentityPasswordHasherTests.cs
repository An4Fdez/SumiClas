using SumiClas.Users.Domain;
using SumiClas.Users.Infrastructure;

namespace SumiClas.Tests.Users.Infrastructure;

[TestFixture]
public class IdentityPasswordHasherTests
{
    private IdentityPasswordHasher _hasher = null!;

    [SetUp]
    public void SetUp() => _hasher = new IdentityPasswordHasher();

    [Test]
    public void Generate_DoesNotContainThePassword()
    {
        var hash = _hasher.Generate(Password.Create("correct horse battery staple").Value);

        Assert.That(hash, Does.Not.Contain("correct horse"));
        Assert.That(hash, Is.Not.Empty);
    }

    [Test]
    public void Generate_OfTheSamePasswordTwice_DiffersBecauseOfTheRandomSalt()
    {
        Assert.That(_hasher.Generate(Password.Create("same-password").Value), Is.Not.EqualTo(_hasher.Generate(Password.Create("same-password").Value)));
    }

    [Test]
    public void Verify_WithTheRightPassword_Succeeds()
    {
        var hash = _hasher.Generate(Password.Create("right-password").Value);

        Assert.That(_hasher.Verify(hash, "right-password"), Is.EqualTo(PasswordVerification.Success));
    }

    [Test]
    public void Verify_WithAWrongPassword_Fails()
    {
        var hash = _hasher.Generate(Password.Create("right-password").Value);

        Assert.That(_hasher.Verify(hash, "wrong-password"), Is.EqualTo(PasswordVerification.Failed));
    }

    [Test]
    public void Verify_WithoutAHash_AlwaysFails()
    {
        Assert.That(_hasher.Verify(null, "anything-at-all"), Is.EqualTo(PasswordVerification.Failed));
    }

    [Test]
    public void Verify_WithAGarbageHash_FailsInsteadOfThrowing()
    {
        Assert.That(_hasher.Verify("not-a-real-hash", "whatever-password"), Is.EqualTo(PasswordVerification.Failed));
    }
}
