using SumiClas.Authentication.Application;
using SumiClas.Authentication.Infrastructure;

namespace SumiClas.Tests.Authentication.Infrastructure;

[TestFixture]
public class IdentityPasswordHasherTests
{
    private IdentityPasswordHasher _hasher = null!;

    [SetUp]
    public void SetUp() => _hasher = new IdentityPasswordHasher();

    [Test]
    public void Hash_DoesNotContainThePassword()
    {
        var hash = _hasher.Hash("correct horse battery staple");

        Assert.That(hash, Does.Not.Contain("correct horse"));
        Assert.That(hash, Is.Not.Empty);
    }

    [Test]
    public void Hash_OfTheSamePasswordTwice_DiffersBecauseOfTheRandomSalt()
    {
        Assert.That(_hasher.Hash("same-password"), Is.Not.EqualTo(_hasher.Hash("same-password")));
    }

    [Test]
    public void Verify_WithTheRightPassword_Succeeds()
    {
        var hash = _hasher.Hash("right-password");

        Assert.That(_hasher.Verify(hash, "right-password"), Is.EqualTo(PasswordVerification.Success));
    }

    [Test]
    public void Verify_WithAWrongPassword_Fails()
    {
        var hash = _hasher.Hash("right-password");

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
