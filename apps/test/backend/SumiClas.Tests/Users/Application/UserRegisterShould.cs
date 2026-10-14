namespace SumiClas.Tests.Users.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class UserRegisterShould : UsersUnitTestCase
{
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly UserRegister.Register _useCase;

    public UserRegisterShould()
    {
        this._useCase = new UserRegister.Register(this.Repository.Object, this._hasher.Object, this.Clock.Object);
    }

    [SetUp]
    public void SetUp()
    {
        this._hasher.Reset();
    }

    [Test]
    public async Task store_the_user_with_the_normalized_data_and_the_hash_and_never_the_password()
    {
        this.ShouldHashPasswords();
        Guid userId = GuidMother.Random();
        string fullName = StringMother.Random("name");
        string email = StringMother.Random("user") + "@example.com";
        string password = StringMother.Random("password");
        this.ShouldSearchNoUsers();
        this.ShouldCaptureSavedUsers();

        Result result = await this._useCase.ExecuteAsync(UserRegisterCommandMother.With(
            userId,
            "  " + fullName + " ",
            " " + email.ToUpperInvariant() + " ",
            password));

        result.IsSuccess.Should().BeTrue();
        User user = this.Saved.Single();
        user.Id.Should().Be(userId);
        user.FullName.Should().Be(fullName);
        user.Email.Value.Should().Be(email);
        user.PasswordHash.Should().Be(HashFor(password));
        user.PasswordChangedAt.Should().Be(Now);
        user.DomainEvents.Should().BeEmpty();
    }

    [Test]
    public async Task return_password_too_weak_without_hashing_when_the_password_is_too_short()
    {
        Result result = await this._useCase.ExecuteAsync(UserRegisterCommandMother.WithPassword("1234567"));

        result.Error.Should().Be(AuthenticationErrors.PasswordTooWeak);
        this._hasher.Verify(hasher => hasher.Generate(It.IsAny<Password>()), Times.Never);
        this.ShouldNotHaveSaved();
    }

    [Test]
    public async Task return_password_too_weak_when_the_password_is_too_long()
    {
        Result result = await this._useCase.ExecuteAsync(
            UserRegisterCommandMother.WithPassword(new string('a', Password.MaxLength + 1)));

        result.Error.Should().Be(AuthenticationErrors.PasswordTooWeak);
        this.ShouldNotHaveSaved();
    }

    [TestCase(8)]
    [TestCase(128)]
    public async Task accept_the_password_when_its_length_is_at_the_limit(int length)
    {
        this.ShouldSearchNoUsers();
        this.ShouldCaptureSavedUsers();

        this.ShouldHashPasswords();
        Result result = await this._useCase.ExecuteAsync(
            UserRegisterCommandMother.WithPassword(new string('a', length)));

        result.IsSuccess.Should().BeTrue();
        this.Saved.Single().PasswordHash.Should().Be($"hash-of-{length}-chars");
    }

    [Test]
    public async Task hash_the_password_as_typed_without_trimming_when_it_is_only_spaces()
    {
        this.ShouldSearchNoUsers();
        this.ShouldCaptureSavedUsers();

        this.ShouldHashPasswords();
        Result result = await this._useCase.ExecuteAsync(UserRegisterCommandMother.WithPassword("        "));

        result.IsSuccess.Should().BeTrue();
        this.Saved.Single().PasswordHash.Should().Be("hash-of-8-chars");
    }

    [Test]
    public async Task return_password_hash_required_when_the_hasher_returns_a_blank_hash()
    {
        this.ShouldGenerateHash("   ");

        Result result = await this._useCase.ExecuteAsync(UserRegisterCommandMother.Random());

        result.Error.Should().Be(UserErrors.PasswordHashRequired);
        this.ShouldNotHaveSaved();
    }

    [Test]
    public async Task return_id_required_when_the_id_is_empty()
    {
        this.ShouldHashPasswords();
        Result result = await this._useCase.ExecuteAsync(UserRegisterCommandMother.WithId(Guid.Empty));

        result.Error.Should().Be(UserErrors.IdRequired);
        this.ShouldNotHaveSaved();
    }

    [Test]
    public async Task fail_with_email_already_registered_and_add_nothing_when_the_email_is_taken()
    {
        this.ShouldHashPasswords();
        string email = StringMother.Random("user") + "@example.com";
        this.ShouldSearchUsers(UserMother.WithEmail(email));

        Result result = await this._useCase.ExecuteAsync(
            UserRegisterCommandMother.WithEmail(email.ToUpperInvariant()));

        result.Error.Should().Be(UserErrors.EmailAlreadyRegistered);
        this.ShouldNotHaveSaved();
    }

    [Test]
    public async Task fail_with_invalid_email_format_without_hashing_when_the_email_is_invalid()
    {
        Result result = await this._useCase.ExecuteAsync(UserRegisterCommandMother.WithEmail("not-an-email"));

        result.Error.Should().Be(UserErrors.InvalidEmailFormat);
        this._hasher.Verify(hasher => hasher.Generate(It.IsAny<Password>()), Times.Never);
        this.ShouldNotHaveSaved();
    }

    [Test]
    public async Task fail_with_full_name_required_when_the_name_is_blank()
    {
        this.ShouldHashPasswords();
        Result result = await this._useCase.ExecuteAsync(UserRegisterCommandMother.WithFullName("   "));

        result.Error.Should().Be(UserErrors.FullNameRequired);
        this.ShouldNotHaveSaved();
    }

    private static string HashFor(string password) => $"hash-of-{password.Length}-chars";

    private void ShouldGenerateHash(string hash)
        => this._hasher
            .Setup(hasher => hasher.Generate(It.IsAny<Password>()))
            .Returns(hash);

    private void ShouldHashPasswords()
        => this._hasher
            .Setup(hasher => hasher.Generate(It.IsAny<Password>()))
            .Returns<Password>(password => HashFor(password.Value));
}
