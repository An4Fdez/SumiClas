using Moq;
using SumiClas.Shared.Domain;
using SumiClas.Shared.Infrastructure.Persistence;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

namespace SumiClas.Tests.Users.Application;

[TestFixture]
public class UserRegisterTests
{
    private static readonly Guid UserId = Guid.Parse("3f2b1c7e-8d4a-4c1e-9b5f-0a6d2e7c9b10");
    private static readonly DateTimeOffset Now = new(2026, 10, 10, 12, 0, 0, TimeSpan.Zero);

    private List<User> _store = null!;
    private Mock<IUserRepository> _users = null!;
    private Mock<IPasswordHasher> _hasher = null!;
    private UserRegister.Register _useCase = null!;
    private UserRegister.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _store = [];
        _users = new Mock<IUserRepository>();
        _users
            .Setup(repository => repository.SearchAsync(It.IsAny<ISpecification<User>>(), It.IsAny<CancellationToken>()))
            .Returns<ISpecification<User>, CancellationToken>((specification, _) =>
                Task.FromResult<IReadOnlyCollection<User>>(
                    SpecificationConverter<User>.GetQuery(_store.AsQueryable(), specification).ToList()));
        _users
            .Setup(repository => repository.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => _store.Add(user))
            .Returns(Task.CompletedTask);

        _hasher = new Mock<IPasswordHasher>();
        _hasher.Setup(hasher => hasher.Generate(It.IsAny<Password>())).Returns<Password>(password => $"hash-of-{password.Value.Length}-chars");

        var clock = new Mock<IClock>();
        clock.Setup(c => c.UtcNow).Returns(Now);

        _useCase = new UserRegister.Register(_users.Object, _hasher.Object, clock.Object);
        _handler = new UserRegister.Handler(_useCase);
    }

    [Test]
    public async Task UseCase_WithValidCommand_StoresTheHashNeverThePassword()
    {
        var command = new UserRegister.Command(UserId, "Ana Perez", "ana@example.com", "a-long-password");

        var result = await _useCase.ExecuteAsync(command);

        Assert.That(result.IsSuccess, Is.True);
        var user = _store.Single();
        Assert.That(user.Id, Is.EqualTo(UserId));
        Assert.That(user.PasswordHash, Is.EqualTo("hash-of-15-chars"));
        Assert.That(user.PasswordHash, Does.Not.Contain("a-long-password"));
        Assert.That(user.DomainEvents, Is.Empty);
    }

    [Test]
    public async Task UseCase_CreatesTheUserWithTheIdNameAndNormalizedEmail()
    {
        var result = await _useCase.ExecuteAsync(new UserRegister.Command(UserId, "  Ana Perez ", " ANA@Example.com ", "a-long-password"));

        Assert.That(result.IsSuccess, Is.True);
        var user = _store.Single();
        Assert.That(user.Id, Is.EqualTo(UserId));
        Assert.That(user.FullName, Is.EqualTo("Ana Perez"));
        Assert.That(user.Email.Value, Is.EqualTo("ana@example.com"));
    }

    [TestCase("")]
    [TestCase("1234567")]
    public async Task UseCase_WithTooShortPassword_ReturnsPasswordTooWeakWithoutHashing(string password)
    {
        var result = await _useCase.ExecuteAsync(new UserRegister.Command(UserId, "Ana Perez", "ana@example.com", password));

        Assert.That(result.Error, Is.EqualTo(AuthenticationErrors.PasswordTooWeak));
        _hasher.Verify(hasher => hasher.Generate(It.IsAny<Password>()), Times.Never);
        Assert.That(_store, Is.Empty);
    }

    [Test]
    public async Task UseCase_WithTooLongPassword_ReturnsPasswordTooWeak()
    {
        var password = new string('a', Password.MaxLength + 1);

        var result = await _useCase.ExecuteAsync(new UserRegister.Command(UserId, "Ana Perez", "ana@example.com", password));

        Assert.That(result.Error, Is.EqualTo(AuthenticationErrors.PasswordTooWeak));
        Assert.That(_store, Is.Empty);
    }

    [Test]
    public async Task UseCase_WithEmptyId_ReturnsIdRequired()
    {
        var result = await _useCase.ExecuteAsync(new UserRegister.Command(Guid.Empty, "Ana Perez", "ana@example.com", "a-long-password"));

        Assert.That(result.Error, Is.EqualTo(UserErrors.IdRequired));
        Assert.That(_store, Is.Empty);
    }

    [Test]
    public async Task UseCase_WithAnEmailAlreadyRegistered_FailsWithEmailAlreadyRegisteredAndAddsNothing()
    {
        _store.Add(User.Register(Guid.NewGuid(), "Other", Email.Create("ana@example.com").Value, "hash", Mock.Of<IClock>()).Value);

        var result = await _useCase.ExecuteAsync(new UserRegister.Command(UserId, "Ana Perez", "ANA@example.com", "a-long-password"));

        Assert.That(result.Error, Is.EqualTo(UserErrors.EmailAlreadyRegistered));
        Assert.That(_store, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task UseCase_WithAnInvalidEmail_FailsWithInvalidEmailFormatWithoutHashing()
    {
        var result = await _useCase.ExecuteAsync(new UserRegister.Command(UserId, "Ana Perez", "not-an-email", "a-long-password"));

        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidEmailFormat));
        _hasher.Verify(hasher => hasher.Generate(It.IsAny<Password>()), Times.Never);
        Assert.That(_store, Is.Empty);
    }

    [Test]
    public async Task UseCase_WithABlankName_FailsWithFullNameRequired()
    {
        var result = await _useCase.ExecuteAsync(new UserRegister.Command(UserId, "   ", "ana@example.com", "a-long-password"));

        Assert.That(result.Error, Is.EqualTo(UserErrors.FullNameRequired));
        Assert.That(_store, Is.Empty);
    }

    [Test]
    public async Task Handler_DelegatesToTheUseCase()
    {
        var result = await _handler.Handle(
            new UserRegister.Command(UserId, "Ana Perez", "ana@example.com", "a-long-password"), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(_store, Has.Count.EqualTo(1));
    }
}
