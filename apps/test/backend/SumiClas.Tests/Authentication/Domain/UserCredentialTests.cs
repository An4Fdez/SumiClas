using Moq;
using SumiClas.Authentication.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Shared.Domain.Events;

namespace SumiClas.Tests.Authentication.Domain;

[TestFixture]
public class UserCredentialTests
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

    [Test]
    public void Create_WithValidData_KeepsTheHashAndStampsBothDates()
    {
        var result = UserCredential.Create(UserId, "hash-value", "Ana Perez", "ana@example.com", _clock.Object);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo(UserId));
        Assert.That(result.Value.PasswordHash, Is.EqualTo("hash-value"));
        Assert.That(result.Value.CreatedAt, Is.EqualTo(Now));
        Assert.That(result.Value.ChangedAt, Is.EqualTo(Now));
    }

    [Test]
    public void Create_RaisesTheCreatedEventWithoutAnySecret()
    {
        var credential = UserCredential.Create(UserId, "hash-value", "Ana Perez", "ana@example.com", _clock.Object).Value;

        var domainEvent = credential.DomainEvents.OfType<UserCredentialCreatedDomainEvent>().Single();
        Assert.That(domainEvent.UserId, Is.EqualTo(UserId));
        Assert.That(domainEvent.FullName, Is.EqualTo("Ana Perez"));
        Assert.That(domainEvent.Email, Is.EqualTo("ana@example.com"));
        Assert.That(domainEvent.OccurredOn, Is.EqualTo(Now));
        Assert.That(
            typeof(UserCredentialCreatedDomainEvent).GetProperties().Select(property => property.Name),
            Has.None.Matches<string>(name => name.Contains("Password") || name.Contains("Hash")));
    }

    [Test]
    public void Create_WithEmptyId_ReturnsIdRequiredAndRaisesNothing()
    {
        var result = UserCredential.Create(Guid.Empty, "hash-value", "Ana Perez", "ana@example.com", _clock.Object);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserCredentialErrors.IdRequired));
    }

    [TestCase("")]
    [TestCase("   ")]
    public void Create_WithBlankHash_ReturnsPasswordHashRequired(string hash)
    {
        var result = UserCredential.Create(UserId, hash, "Ana Perez", "ana@example.com", _clock.Object);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(UserCredentialErrors.PasswordHashRequired));
    }

    [Test]
    public void ReplacePasswordHash_StoresTheNewHashAndMovesOnlyChangedAt()
    {
        var credential = UserCredential.Create(UserId, "old", "Ana Perez", "ana@example.com", _clock.Object).Value;
        var later = Now.AddDays(3);
        _clock.Setup(clock => clock.UtcNow).Returns(later);

        var result = credential.ReplacePasswordHash("new", _clock.Object);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(credential.PasswordHash, Is.EqualTo("new"));
        Assert.That(credential.CreatedAt, Is.EqualTo(Now));
        Assert.That(credential.ChangedAt, Is.EqualTo(later));
    }

    [Test]
    public void ReplacePasswordHash_WithBlankHash_KeepsTheCurrentOne()
    {
        var credential = UserCredential.Create(UserId, "old", "Ana Perez", "ana@example.com", _clock.Object).Value;

        var result = credential.ReplacePasswordHash(" ", _clock.Object);

        Assert.That(result.Error, Is.EqualTo(UserCredentialErrors.PasswordHashRequired));
        Assert.That(credential.PasswordHash, Is.EqualTo("old"));
    }

    [TestCase("12345678", true)]
    [TestCase("1234567", false)]
    [TestCase("", false)]
    [TestCase(null, false)]
    public void PasswordPolicy_EnforcesTheMinimumLength(string? password, bool valid)
    {
        var result = PasswordPolicy.Validate(password);

        Assert.That(result.IsSuccess, Is.EqualTo(valid));
        if (!valid) Assert.That(result.Error, Is.EqualTo(AuthenticationErrors.PasswordTooWeak));
    }

    [Test]
    public void PasswordPolicy_RejectsMoreThanTheMaximumLength()
    {
        Assert.That(PasswordPolicy.Validate(new string('a', PasswordPolicy.MaxLength)).IsSuccess, Is.True);
        Assert.That(PasswordPolicy.Validate(new string('a', PasswordPolicy.MaxLength + 1)).Error, Is.EqualTo(AuthenticationErrors.PasswordTooWeak));
    }
}
