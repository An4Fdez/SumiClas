namespace SumiClas.Tests.Users.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class UserPasswordVerifyShould : UsersUnitTestCase
{
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly UserPasswordVerify.Verify _useCase;

    public UserPasswordVerifyShould()
    {
        this._useCase = new UserPasswordVerify.Verify(this.Repository.Object, this._hasher.Object, this.Clock.Object);
    }

    [SetUp]
    public void SetUp()
    {
        this._hasher.Reset();
    }

    [Test]
    public async Task return_the_user_when_the_password_is_right()
    {
        Guid userId = GuidMother.Random();
        string fullName = StringMother.Random("name");
        string email = StringMother.Random("user") + "@example.com";
        string password = StringMother.Random("password");
        string storedHash = StringMother.Random("stored");
        this.GivenAnAccount(userId, fullName, email, storedHash);
        this.ShouldVerifyPassword(storedHash, password, PasswordVerification.Success);

        Result<UserResponse> result = await this._useCase.ExecuteAsync(
            UserPasswordVerifyQueryMother.WithCredentials(email, password),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(UserResponseMother.Create(userId, fullName));
        this._hasher.Verify(hasher => hasher.Generate(It.IsAny<Password>()), Times.Never);
    }

    [Test]
    public async Task return_invalid_credentials_when_the_password_is_wrong()
    {
        string email = StringMother.Random("user") + "@example.com";
        string wrongPassword = StringMother.Random("wrong");
        string storedHash = StringMother.Random("stored");
        this.GivenAnAccount(GuidMother.Random(), StringMother.Random("name"), email, storedHash);
        this.ShouldVerifyPassword(storedHash, wrongPassword, PasswordVerification.Failed);

        Result<UserResponse> result = await this._useCase.ExecuteAsync(
            UserPasswordVerifyQueryMother.WithCredentials(email, wrongPassword),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthenticationErrors.InvalidCredentials);
    }

    [Test]
    public async Task return_the_same_error_and_still_verify_once_when_the_email_is_unknown()
    {
        string unknownEmail = StringMother.Random("nobody") + "@example.com";
        string unknownPassword = StringMother.Random("password");
        this.ShouldSearchNoUsers();

        Result<UserResponse> result = await this._useCase.ExecuteAsync(
            UserPasswordVerifyQueryMother.WithCredentials(unknownEmail, unknownPassword),
            CancellationToken.None);

        result.Error.Should().Be(AuthenticationErrors.InvalidCredentials);
        this._hasher.Verify(hasher => hasher.Verify(null, unknownPassword), Times.Once);
    }

    [Test]
    public async Task replace_the_hash_on_the_tracked_user_when_the_hasher_asks_for_a_rehash()
    {
        string email = StringMother.Random("user") + "@example.com";
        string password = StringMother.Random("password");
        string oldHash = StringMother.Random("old");
        string newHash = StringMother.Random("new");
        User user = this.GivenAnAccount(GuidMother.Random(), StringMother.Random("name"), email, oldHash);
        this.ShouldGenerateHash(newHash);
        this.ShouldVerifyPassword(oldHash, password, PasswordVerification.SuccessRehashNeeded);
        this.AdvanceClock(TimeSpan.FromDays(1));

        Result<UserResponse> result = await this._useCase.ExecuteAsync(
            UserPasswordVerifyQueryMother.WithCredentials(email, password),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.PasswordHash.Should().Be(newHash);
        user.PasswordChangedAt.Should().Be(Now.AddDays(1));
    }

    [Test]
    public async Task log_in_without_replacing_the_hash_when_a_rehash_is_needed_but_the_password_is_outside_the_policy()
    {
        string email = StringMother.Random("user") + "@example.com";
        string oldHash = StringMother.Random("old");
        User user = this.GivenAnAccount(GuidMother.Random(), StringMother.Random("name"), email, oldHash);
        this.ShouldVerifyPassword(oldHash, "short", PasswordVerification.SuccessRehashNeeded);

        Result<UserResponse> result = await this._useCase.ExecuteAsync(
            UserPasswordVerifyQueryMother.WithCredentials(email, "short"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.PasswordHash.Should().Be(oldHash);
        this._hasher.Verify(hasher => hasher.Generate(It.IsAny<Password>()), Times.Never);
    }

    [Test]
    public async Task fail_as_invalid_credentials_instead_of_throwing_when_the_fields_are_null()
    {
        Result<UserResponse> result = await this._useCase.ExecuteAsync(
            UserPasswordVerifyQueryMother.WithNullFields(),
            CancellationToken.None);

        result.Error.Should().Be(AuthenticationErrors.InvalidCredentials);
        this._hasher.Verify(hasher => hasher.Verify(null, string.Empty), Times.Once);
    }

    private User GivenAnAccount(Guid userId, string fullName, string email, string passwordHash)
    {
        User user = UserMother.With(
            id: userId,
            fullName: fullName,
            email: EmailMother.Create(email),
            passwordHash: passwordHash);
        this.ShouldSearchUsers(user);
        return user;
    }

    private void ShouldVerifyPassword(string storedHash, string password, PasswordVerification verification)
        => this._hasher
            .Setup(hasher => hasher.Verify(storedHash, password))
            .Returns(verification);

    private void ShouldGenerateHash(string hash)
        => this._hasher.Setup(hasher => hasher.Generate(It.IsAny<Password>())).Returns(hash);
}
