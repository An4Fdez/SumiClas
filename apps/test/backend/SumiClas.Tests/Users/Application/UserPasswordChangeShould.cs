namespace SumiClas.Tests.Users.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.Shared.Domain;
using SumiClas.Shared.Domain.Events;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

[TestFixture]
public class UserPasswordChangeShould : UsersUnitTestCase
{
    private const string STORED_HASH = "stored-hash";
    private const string NEW_HASH = "new-hash";
    private const string TOO_SHORT_PASSWORD = "1234567";

    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly UserPasswordChange.Change _useCase;

    public UserPasswordChangeShould()
    {
        this._useCase = new UserPasswordChange.Change(
            this.Repository.Object,
            this._hasher.Object,
            this.UserProvider.Object,
            this.Clock.Object);
    }

    [SetUp]
    public void SetUp()
    {
        this._hasher.Reset();
    }

    [Test]
    public async Task store_the_hash_of_the_new_password_and_announce_the_change()
    {
        User current = UserMother.With(passwordHash: STORED_HASH);
        string currentPassword = StringMother.Random("current");
        string newPassword = StringMother.Random("new-password");
        TimeSpan elapsed = TimeSpan.FromHours(1);
        this.ShouldFetchUsers(current);
        this.ShouldResolveCurrentUser(current.Id);
        this.ShouldVerify(STORED_HASH, currentPassword, PasswordVerification.Success);
        this.ShouldGenerate(newPassword, NEW_HASH);
        this.AdvanceClock(elapsed);

        await this._useCase.ExecuteAsync(UserPasswordChangeCommandMother.With(currentPassword, newPassword));

        current.PasswordHash.Should().Be(NEW_HASH);
        current.PasswordChangedAt.Should().Be(Now + elapsed);
        current.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<UserPasswordChangedDomainEvent>()
            .Which.UserId.Should().Be(current.Id);
    }

    [Test]
    public async Task accept_a_current_password_whose_hash_needs_a_rehash()
    {
        User current = UserMother.With(passwordHash: STORED_HASH);
        string currentPassword = StringMother.Random("current");
        string newPassword = StringMother.Random("new-password");
        this.ShouldFetchUsers(current);
        this.ShouldResolveCurrentUser(current.Id);
        this.ShouldVerify(STORED_HASH, currentPassword, PasswordVerification.SuccessRehashNeeded);
        this.ShouldGenerate(newPassword, NEW_HASH);

        await this._useCase.ExecuteAsync(UserPasswordChangeCommandMother.With(currentPassword, newPassword));

        current.PasswordHash.Should().Be(NEW_HASH);
    }

    [Test]
    public async Task throw_current_password_invalid_and_keep_everything_when_the_current_password_is_wrong()
    {
        User current = UserMother.With(passwordHash: STORED_HASH);
        string wrongPassword = StringMother.Random("wrong");
        this.ShouldFetchUsers(current);
        this.ShouldResolveCurrentUser(current.Id);
        this.ShouldVerify(STORED_HASH, wrongPassword, PasswordVerification.Failed);

        Func<Task> act = () => this._useCase.ExecuteAsync(UserPasswordChangeCommandMother.With(wrongPassword));

        await act.Should().ThrowAsync<UserExceptions.CurrentPasswordInvalid>();
        current.PasswordHash.Should().Be(STORED_HASH);
        current.DomainEvents.Should().BeEmpty();
        this._hasher.Verify(hasher => hasher.Generate(It.IsAny<Password>()), Times.Never);
    }

    [Test]
    public async Task throw_password_too_weak_without_checking_or_hashing_when_the_new_password_breaks_the_policy()
    {
        User current = UserMother.With(passwordHash: STORED_HASH);
        this.ShouldFetchUsers(current);
        this.ShouldResolveCurrentUser(current.Id);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            UserPasswordChangeCommandMother.With(newPassword: TOO_SHORT_PASSWORD));

        await act.Should().ThrowAsync<AuthenticationExceptions.PasswordTooWeak>();
        current.PasswordHash.Should().Be(STORED_HASH);
        current.DomainEvents.Should().BeEmpty();
        this._hasher.Verify(hasher => hasher.Verify(It.IsAny<string?>(), It.IsAny<string>()), Times.Never);
        this._hasher.Verify(hasher => hasher.Generate(It.IsAny<Password>()), Times.Never);
    }

    [Test]
    public async Task throw_not_found_when_the_current_user_does_not_exist()
    {
        this.ShouldFetchNoUsers();
        this.ShouldResolveCurrentUser(GuidMother.Random());

        Func<Task> act = () => this._useCase.ExecuteAsync(UserPasswordChangeCommandMother.With());

        await act.Should().ThrowAsync<UserExceptions.NotFound>();
    }

    [Test]
    public async Task throw_unauthenticated_without_fetching_the_user_when_there_is_no_current_user()
    {
        this.ShouldHaveNoCurrentUser();

        Func<Task> act = () => this._useCase.ExecuteAsync(UserPasswordChangeCommandMother.With());

        await act.Should().ThrowAsync<AuthenticationExceptions.Unauthenticated>();
        this.ShouldNotHaveFetched();
    }

    private void ShouldVerify(string storedHash, string password, PasswordVerification result)
        => this._hasher
            .Setup(hasher => hasher.Verify(storedHash, password))
            .Returns(result);

    private void ShouldGenerate(string password, string hash)
        => this._hasher
            .Setup(hasher => hasher.Generate(It.Is<Password>(candidate => candidate.Value == password)))
            .Returns(hash);
}
