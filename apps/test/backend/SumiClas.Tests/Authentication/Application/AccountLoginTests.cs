using Moq;
using SumiClas.Authentication.Application;
using SumiClas.Authentication.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

namespace SumiClas.Tests.Authentication.Application;

[TestFixture]
public class AccountLoginTests
{
    private static readonly Guid UserId = Guid.Parse("3f2b1c7e-8d4a-4c1e-9b5f-0a6d2e7c9b10");
    private static readonly DateTimeOffset Now = new(2026, 10, 10, 12, 0, 0, TimeSpan.Zero);

    private Mock<IQueryBus> _queryBus = null!;
    private Mock<IUserCredentialRepository> _credentials = null!;
    private Mock<IPasswordHasher> _hasher = null!;
    private Mock<IClock> _clock = null!;
    private AccountLogin.UseCase _useCase = null!;

    [SetUp]
    public void SetUp()
    {
        _clock = new Mock<IClock>();
        _clock.Setup(c => c.UtcNow).Returns(Now);

        _queryBus = new Mock<IQueryBus>();
        _queryBus
            .Setup(bus => bus.Send(It.IsAny<UserFinderByEmail.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserResponse>.Failure(UserErrors.NotFound));

        _credentials = new Mock<IUserCredentialRepository>();
        _hasher = new Mock<IPasswordHasher>();
        _hasher.Setup(hasher => hasher.Hash(It.IsAny<string>())).Returns("new-hash");

        _useCase = new AccountLogin.UseCase(_queryBus.Object, _credentials.Object, _hasher.Object, _clock.Object);
    }

    private UserCredential GivenAnAccount(string hash = "stored-hash")
    {
        _queryBus
            .Setup(bus => bus.Send(It.Is<UserFinderByEmail.Query>(query => query.Email == "ana@example.com"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserResponse>.Success(new UserResponse(UserId, "Ana Perez")));
        var credential = UserCredential.Create(UserId, hash, "Ana Perez", "ana@example.com", _clock.Object).Value;
        _credentials
            .Setup(repository => repository.GetByIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(credential);
        return credential;
    }

    [Test]
    public async Task UseCase_WithRightPassword_ReturnsTheUserIdAndName()
    {
        GivenAnAccount();
        _hasher.Setup(hasher => hasher.Verify("stored-hash", "right-password")).Returns(PasswordVerification.Success);

        var result = await _useCase.ExecuteAsync(new AccountLogin.Command("ana@example.com", "right-password"));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(new AccountResponse(UserId, "Ana Perez")));
        _hasher.Verify(hasher => hasher.Hash(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task UseCase_WithWrongPassword_ReturnsInvalidCredentials()
    {
        GivenAnAccount();
        _hasher.Setup(hasher => hasher.Verify("stored-hash", "wrong")).Returns(PasswordVerification.Failed);

        var result = await _useCase.ExecuteAsync(new AccountLogin.Command("ana@example.com", "wrong"));

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(AuthenticationErrors.InvalidCredentials));
    }

    [Test]
    public async Task UseCase_WithUnknownEmail_ReturnsTheSameErrorAndStillVerifiesOnce()
    {
        var result = await _useCase.ExecuteAsync(new AccountLogin.Command("nobody@example.com", "whatever"));

        Assert.That(result.Error, Is.EqualTo(AuthenticationErrors.InvalidCredentials));
        _hasher.Verify(hasher => hasher.Verify(null, "whatever"), Times.Once);
        _credentials.Verify(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task UseCase_WithAUserWithoutCredential_ReturnsTheSameError()
    {
        _queryBus
            .Setup(bus => bus.Send(It.IsAny<UserFinderByEmail.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserResponse>.Success(new UserResponse(UserId, "Ana Perez")));
        _credentials
            .Setup(repository => repository.GetByIdAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserCredential?)null);

        var result = await _useCase.ExecuteAsync(new AccountLogin.Command("ana@example.com", "whatever"));

        Assert.That(result.Error, Is.EqualTo(AuthenticationErrors.InvalidCredentials));
        _hasher.Verify(hasher => hasher.Verify(null, "whatever"), Times.Once);
    }

    [Test]
    public async Task UseCase_WhenTheHasherAsksForARehash_StoresTheNewHash()
    {
        var credential = GivenAnAccount("old-hash");
        _hasher.Setup(hasher => hasher.Verify("old-hash", "right-password")).Returns(PasswordVerification.SuccessRehashNeeded);
        var later = Now.AddDays(1);
        _clock.Setup(c => c.UtcNow).Returns(later);

        var result = await _useCase.ExecuteAsync(new AccountLogin.Command("ana@example.com", "right-password"));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(credential.PasswordHash, Is.EqualTo("new-hash"));
        Assert.That(credential.ChangedAt, Is.EqualTo(later));
    }

    [Test]
    public async Task UseCase_WithNullFields_FailsAsInvalidCredentialsInsteadOfThrowing()
    {
        var result = await _useCase.ExecuteAsync(new AccountLogin.Command(null!, null!));

        Assert.That(result.Error, Is.EqualTo(AuthenticationErrors.InvalidCredentials));
    }

    [Test]
    public async Task Handler_DelegatesToTheUseCase()
    {
        var handler = new AccountLogin.Handler(_useCase);

        var result = await handler.Handle(new AccountLogin.Command("nobody@example.com", "whatever"), CancellationToken.None);

        Assert.That(result.Error, Is.EqualTo(AuthenticationErrors.InvalidCredentials));
    }
}
