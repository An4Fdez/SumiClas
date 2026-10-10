using Moq;
using SumiClas.Shared.Domain;
using SumiClas.Shared.Domain.Events;
using SumiClas.Shared.Infrastructure.Persistence;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

namespace SumiClas.Tests.Users.Application;

[TestFixture]
public class UserRegisterOnUserCredentialCreatedTests
{
    private static readonly Guid UserId = Guid.Parse("3f2b1c7e-8d4a-4c1e-9b5f-0a6d2e7c9b10");

    private List<User> _store = null!;
    private Mock<IUserRepository> _users = null!;
    private Mock<IClock> _clock = null!;
    private UserRegisterOnUserCredentialCreated _handler = null!;

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

        _clock = new Mock<IClock>();
        _clock.Setup(c => c.UtcNow).Returns(DateTimeOffset.UnixEpoch);

        _handler = new UserRegisterOnUserCredentialCreated(_users.Object);
    }

    private UserCredentialCreatedDomainEvent EventFor(Guid userId, string fullName, string email)
        => new(userId, fullName, email, _clock.Object);

    [Test]
    public async Task Handle_CreatesTheUserWithTheIdNameAndNormalizedEmail()
    {
        var result = await _handler.Handle(EventFor(UserId, "  Ana Perez ", " ANA@Example.com "));

        Assert.That(result.IsSuccess, Is.True);
        var user = _store.Single();
        Assert.That(user.Id, Is.EqualTo(UserId));
        Assert.That(user.FullName, Is.EqualTo("Ana Perez"));
        Assert.That(user.Email.Value, Is.EqualTo("ana@example.com"));
    }

    [Test]
    public async Task Handle_WithAnEmailAlreadyRegistered_FailsWithEmailAlreadyRegisteredAndAddsNothing()
    {
        _store.Add(User.Register(Guid.NewGuid(), "Other", Email.Create("ana@example.com").Value).Value);

        var result = await _handler.Handle(EventFor(UserId, "Ana Perez", "ANA@example.com"));

        Assert.That(result.Error, Is.EqualTo(UserErrors.EmailAlreadyRegistered));
        Assert.That(_store, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Handle_WithAnInvalidEmail_FailsWithInvalidEmailFormat()
    {
        var result = await _handler.Handle(EventFor(UserId, "Ana Perez", "not-an-email"));

        Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidEmailFormat));
        Assert.That(_store, Is.Empty);
    }

    [Test]
    public async Task Handle_WithABlankName_FailsWithFullNameRequired()
    {
        var result = await _handler.Handle(EventFor(UserId, "   ", "ana@example.com"));

        Assert.That(result.Error, Is.EqualTo(UserErrors.FullNameRequired));
        Assert.That(_store, Is.Empty);
    }
}
