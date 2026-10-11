using Moq;
using SumiClas.Shared.Domain;
using SumiClas.Shared.Infrastructure.Persistence;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

namespace SumiClas.Tests.Users.Application;

[TestFixture]
public class UserPasswordVerifyTests
{
    private static readonly Guid UserId = Guid.Parse("3f2b1c7e-8d4a-4c1e-9b5f-0a6d2e7c9b10");
    private static readonly DateTimeOffset Now = new(2026, 10, 10, 12, 0, 0, TimeSpan.Zero);

    private List<User> _store = null!;
    private Mock<IUserRepository> _users = null!;
    private Mock<IPasswordHasher> _hasher = null!;
    private Mock<IClock> _clock = null!;
    private UserPasswordVerify.Verify _useCase = null!;
    private UserPasswordVerify.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _clock = new Mock<IClock>();
        _clock.Setup(c => c.UtcNow).Returns(Now);

        _store = [];
        _users = new Mock<IUserRepository>();
        _users
            .Setup(repository => repository.SearchAsync(It.IsAny<ISpecification<User>>(), It.IsAny<CancellationToken>()))
            .Returns<ISpecification<User>, CancellationToken>((specification, _) =>
                Task.FromResult<IReadOnlyCollection<User>>(
                    SpecificationConverter<User>.GetQuery(_store.AsQueryable(), specification).ToList()));

        _hasher = new Mock<IPasswordHasher>();
        _hasher.Setup(hasher => hasher.Generate(It.IsAny<Password>())).Returns("new-hash");

        _useCase = new UserPasswordVerify.Verify(_users.Object, _hasher.Object, _clock.Object);
        _handler = new UserPasswordVerify.Handler(_useCase);
    }

    private User GivenAnAccount(string hash = "stored-hash")
    {
        var user = User.Register(UserId, "Ana Perez", Email.Create("ana@example.com").Value, hash, _clock.Object).Value;
        _store.Add(user);
        return user;
    }

    [Test]
    public async Task UseCase_WithRightPassword_ReturnsTheUser()
    {
        GivenAnAccount();
        _hasher.Setup(hasher => hasher.Verify("stored-hash", "right-password")).Returns(PasswordVerification.Success);

        var result = await _useCase.ExecuteAsync(new UserPasswordVerify.Query("ana@example.com", "right-password"), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(new UserResponse(UserId, "Ana Perez")));
        _hasher.Verify(hasher => hasher.Generate(It.IsAny<Password>()), Times.Never);
    }

    [Test]
    public async Task UseCase_WithWrongPassword_ReturnsInvalidCredentials()
    {
        GivenAnAccount();
        _hasher.Setup(hasher => hasher.Verify("stored-hash", "wrong")).Returns(PasswordVerification.Failed);

        var result = await _useCase.ExecuteAsync(new UserPasswordVerify.Query("ana@example.com", "wrong"), CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(AuthenticationErrors.InvalidCredentials));
    }

    [Test]
    public async Task UseCase_WithUnknownEmail_ReturnsTheSameErrorAndStillVerifiesOnce()
    {
        var result = await _useCase.ExecuteAsync(new UserPasswordVerify.Query("nobody@example.com", "whatever"), CancellationToken.None);

        Assert.That(result.Error, Is.EqualTo(AuthenticationErrors.InvalidCredentials));
        _hasher.Verify(hasher => hasher.Verify(null, "whatever"), Times.Once);
    }

    [Test]
    public async Task UseCase_WhenTheHasherAsksForARehash_ReplacesTheHashOnTheTrackedUser()
    {
        var user = GivenAnAccount("old-hash");
        _hasher.Setup(hasher => hasher.Verify("old-hash", "right-password")).Returns(PasswordVerification.SuccessRehashNeeded);
        var later = Now.AddDays(1);
        _clock.Setup(c => c.UtcNow).Returns(later);

        var result = await _useCase.ExecuteAsync(new UserPasswordVerify.Query("ana@example.com", "right-password"), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(user.PasswordHash, Is.EqualTo("new-hash"));
        Assert.That(user.PasswordChangedAt, Is.EqualTo(later));
    }


    [Test]
    public async Task UseCase_WhenARehashIsNeededButThePasswordIsOutsideThePolicy_StillLogsInWithoutReplacingTheHash()
    {
        var user = GivenAnAccount("old-hash");
        _hasher.Setup(hasher => hasher.Verify("old-hash", "short")).Returns(PasswordVerification.SuccessRehashNeeded);

        var result = await _useCase.ExecuteAsync(new UserPasswordVerify.Query("ana@example.com", "short"), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(user.PasswordHash, Is.EqualTo("old-hash"));
        _hasher.Verify(hasher => hasher.Generate(It.IsAny<Password>()), Times.Never);
    }
    [Test]
    public async Task UseCase_WithNullFields_FailsAsInvalidCredentialsInsteadOfThrowing()
    {
        var result = await _useCase.ExecuteAsync(new UserPasswordVerify.Query(null!, null!), CancellationToken.None);

        Assert.That(result.Error, Is.EqualTo(AuthenticationErrors.InvalidCredentials));
        _hasher.Verify(hasher => hasher.Verify(null, string.Empty), Times.Once);
    }

    [Test]
    public async Task UseCase_AlwaysInvokesTheHasherWhateverTheOutcome()
    {
        await _useCase.ExecuteAsync(new UserPasswordVerify.Query("nobody@example.com", "x"), CancellationToken.None);
        GivenAnAccount();
        _hasher.Setup(hasher => hasher.Verify("stored-hash", "y")).Returns(PasswordVerification.Failed);
        await _useCase.ExecuteAsync(new UserPasswordVerify.Query("ana@example.com", "y"), CancellationToken.None);

        _hasher.Verify(hasher => hasher.Verify(It.IsAny<string?>(), It.IsAny<string>()), Times.Exactly(2));
    }

    [Test]
    public async Task Handler_DelegatesToTheUseCase()
    {
        GivenAnAccount();
        _hasher.Setup(hasher => hasher.Verify("stored-hash", "right-password")).Returns(PasswordVerification.Success);

        var result = await _handler.Handle(new UserPasswordVerify.Query("ana@example.com", "right-password"), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(new UserResponse(UserId, "Ana Perez")));
    }
}
