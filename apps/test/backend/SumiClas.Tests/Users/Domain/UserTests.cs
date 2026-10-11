using Moq;
using SumiClas.Shared.Domain;
using SumiClas.Users.Domain;

namespace SumiClas.Tests.Users.Domain;

[TestFixture]
public class UserTests
{
    private static readonly Guid UserId = Guid.Parse("3f2b1c7e-8d4a-4c1e-9b5f-0a6d2e7c9b10");
    private static readonly DateTimeOffset Now = new(2026, 10, 10, 12, 0, 0, TimeSpan.Zero);

    private Mock<IClock> _clock = null!;

    [SetUp]
    public void SetUp()
    {
        _clock = new Mock<IClock>();
        _clock.Setup(clock => clock.UtcNow).Returns(Now);
    }

    private User GivenAUser(string hash = "hash-value")
        => User.Register(UserId, "Ana Perez", Email.Create("ana@example.com").Value, hash, _clock.Object).Value;

    [Test]
    public void Register_WithValidData_KeepsTheHashAndStampsThePasswordDate()
    {
        var result = User.Register(UserId, "  Ana Perez ", Email.Create("ana@example.com").Value, "hash-value", _clock.Object);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo(UserId));
        Assert.That(result.Value.FullName, Is.EqualTo("Ana Perez"));
        Assert.That(result.Value.PasswordHash, Is.EqualTo("hash-value"));
        Assert.That(result.Value.PasswordChangedAt, Is.EqualTo(Now));
    }

    [Test]
    public void Register_RaisesNoDomainEvent()
    {
        Assert.That(GivenAUser().DomainEvents, Is.Empty);
    }

    [Test]
    public void Register_WithEmptyId_ReturnsIdRequired()
    {
        var result = User.Register(Guid.Empty, "Ana Perez", Email.Create("ana@example.com").Value, "hash-value", _clock.Object);

        Assert.That(result.Error, Is.EqualTo(UserErrors.IdRequired));
    }

    [Test]
    public void Register_WithABlankName_ReturnsFullNameRequired()
    {
        var result = User.Register(UserId, "   ", Email.Create("ana@example.com").Value, "hash-value", _clock.Object);

        Assert.That(result.Error, Is.EqualTo(UserErrors.FullNameRequired));
    }

    [TestCase("")]
    [TestCase("   ")]
    public void Register_WithBlankHash_ReturnsPasswordHashRequired(string hash)
    {
        var result = User.Register(UserId, "Ana Perez", Email.Create("ana@example.com").Value, hash, _clock.Object);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserErrors.PasswordHashRequired));
    }

    [Test]
    public void ReplacePasswordHash_StoresTheNewHashAndMovesThePasswordDate()
    {
        var user = GivenAUser("old");
        var later = Now.AddDays(3);
        _clock.Setup(clock => clock.UtcNow).Returns(later);

        var result = user.ReplacePasswordHash("new", _clock.Object);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(user.PasswordHash, Is.EqualTo("new"));
        Assert.That(user.PasswordChangedAt, Is.EqualTo(later));
    }

    [Test]
    public void ReplacePasswordHash_WithBlankHash_KeepsTheCurrentOne()
    {
        var user = GivenAUser("old");

        var result = user.ReplacePasswordHash(" ", _clock.Object);

        Assert.That(result.Error, Is.EqualTo(UserErrors.PasswordHashRequired));
        Assert.That(user.PasswordHash, Is.EqualTo("old"));
    }
}
