using Moq;
using SumiClas.Authentication.Application;
using SumiClas.Authentication.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Shared.Domain.Events;

namespace SumiClas.Tests.Authentication.Application;

[TestFixture]
public class AccountRegisterTests
{
    private static readonly Guid UserId = Guid.Parse("3f2b1c7e-8d4a-4c1e-9b5f-0a6d2e7c9b10");

    private Mock<IUserCredentialRepository> _credentials = null!;
    private Mock<IPasswordHasher> _hasher = null!;
    private List<UserCredential> _added = null!;
    private AccountRegister.UseCase _useCase = null!;
    private AccountRegister.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _added = [];
        _credentials = new Mock<IUserCredentialRepository>();
        _credentials
            .Setup(repository => repository.AddAsync(It.IsAny<UserCredential>(), It.IsAny<CancellationToken>()))
            .Callback<UserCredential, CancellationToken>((credential, _) => _added.Add(credential))
            .Returns(Task.CompletedTask);

        _hasher = new Mock<IPasswordHasher>();
        _hasher.Setup(hasher => hasher.Hash(It.IsAny<string>())).Returns<string>(password => $"hash-of-{password.Length}-chars");

        var clock = new Mock<IClock>();
        clock.Setup(c => c.UtcNow).Returns(new DateTimeOffset(2026, 10, 10, 12, 0, 0, TimeSpan.Zero));

        _useCase = new AccountRegister.UseCase(_credentials.Object, _hasher.Object, clock.Object);
        _handler = new AccountRegister.Handler(_useCase);
    }

    [Test]
    public async Task UseCase_WithValidCommand_StoresTheHashNeverThePassword()
    {
        var command = new AccountRegister.Command(UserId, "Ana Perez", "ana@example.com", "a-long-password");

        var result = await _useCase.ExecuteAsync(command);

        Assert.That(result.IsSuccess, Is.True);
        var credential = _added.Single();
        Assert.That(credential.Id, Is.EqualTo(UserId));
        Assert.That(credential.PasswordHash, Is.EqualTo("hash-of-15-chars"));
        Assert.That(credential.PasswordHash, Does.Not.Contain("a-long-password"));
    }

    [Test]
    public async Task UseCase_RaisesTheCreatedEventWithNameAndEmailButNoSecret()
    {
        await _useCase.ExecuteAsync(new AccountRegister.Command(UserId, "Ana Perez", "ana@example.com", "a-long-password"));

        var domainEvent = _added.Single().DomainEvents.OfType<UserCredentialCreatedDomainEvent>().Single();
        Assert.That(domainEvent.UserId, Is.EqualTo(UserId));
        Assert.That(domainEvent.FullName, Is.EqualTo("Ana Perez"));
        Assert.That(domainEvent.Email, Is.EqualTo("ana@example.com"));
    }

    [TestCase("")]
    [TestCase("1234567")]
    public async Task UseCase_WithTooShortPassword_ReturnsPasswordTooWeakWithoutHashing(string password)
    {
        var result = await _useCase.ExecuteAsync(new AccountRegister.Command(UserId, "Ana Perez", "ana@example.com", password));

        Assert.That(result.Error, Is.EqualTo(AuthenticationErrors.PasswordTooWeak));
        _hasher.Verify(hasher => hasher.Hash(It.IsAny<string>()), Times.Never);
        Assert.That(_added, Is.Empty);
    }

    [Test]
    public async Task UseCase_WithTooLongPassword_ReturnsPasswordTooWeak()
    {
        var password = new string('a', PasswordPolicy.MaxLength + 1);

        var result = await _useCase.ExecuteAsync(new AccountRegister.Command(UserId, "Ana Perez", "ana@example.com", password));

        Assert.That(result.Error, Is.EqualTo(AuthenticationErrors.PasswordTooWeak));
        Assert.That(_added, Is.Empty);
    }

    [Test]
    public async Task UseCase_WithEmptyId_ReturnsIdRequired()
    {
        var result = await _useCase.ExecuteAsync(new AccountRegister.Command(Guid.Empty, "Ana Perez", "ana@example.com", "a-long-password"));

        Assert.That(result.Error, Is.EqualTo(UserCredentialErrors.IdRequired));
        Assert.That(_added, Is.Empty);
    }

    [Test]
    public async Task Handler_DelegatesToTheUseCase()
    {
        var result = await _handler.Handle(
            new AccountRegister.Command(UserId, "Ana Perez", "ana@example.com", "a-long-password"), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(_added, Has.Count.EqualTo(1));
    }
}
