namespace SumiClas.Tests.Users.Infrastructure;

using AwesomeAssertions;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Domain;
using SumiClas.Users.Infrastructure;

[TestFixture]
public class IdentityPasswordHasherShould
{
    private readonly IdentityPasswordHasher _hasher = new();

    [Test]
    public void not_contain_the_password_in_the_generated_hash()
    {
        string hash = this._hasher.Generate(PasswordMother.Create("correct horse battery staple"));

        hash.Should().NotContain("correct horse");
        hash.Should().NotBeEmpty();
    }

    [Test]
    public void generate_different_hashes_for_the_same_password_because_of_the_random_salt()
    {
        string first = this._hasher.Generate(PasswordMother.Create("same-password"));
        string second = this._hasher.Generate(PasswordMother.Create("same-password"));

        first.Should().NotBe(second);
    }

    [Test]
    public void succeed_when_verifying_with_the_right_password()
    {
        string hash = this._hasher.Generate(PasswordMother.Create("right-password"));

        this._hasher.Verify(hash, "right-password").Should().Be(PasswordVerification.Success);
    }

    [Test]
    public void fail_when_verifying_with_a_wrong_password()
    {
        string hash = this._hasher.Generate(PasswordMother.Create("right-password"));

        this._hasher.Verify(hash, "wrong-password").Should().Be(PasswordVerification.Failed);
    }

    [Test]
    public void always_fail_when_verifying_without_a_hash()
    {
        this._hasher.Verify(null, "anything-at-all").Should().Be(PasswordVerification.Failed);
    }

    [Test]
    public void fail_instead_of_throwing_when_verifying_a_garbage_hash()
    {
        this._hasher.Verify("not-a-real-hash", "whatever-password").Should().Be(PasswordVerification.Failed);
    }
}
