using Moq;
using SumiClas.RefreshTokens.Domain;
using SumiClas.Shared.Domain;

namespace SumiClas.Tests.RefreshTokens.Domain;

[TestFixture]
public class RefreshTokenTests
{
    private static readonly Guid TokenId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid UserId = Guid.Parse("3f2b1c7e-8d4a-4c1e-9b5f-0a6d2e7c9b10");
    private static readonly Guid FamilyId = Guid.Parse("22222222-2222-4222-8222-222222222222");
    private static readonly Guid SuccessorId = Guid.Parse("33333333-3333-4333-8333-333333333333");
    private static readonly DateTimeOffset Now = new(2026, 10, 10, 12, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan Lifetime = TimeSpan.FromDays(14);
    private static readonly TimeSpan Grace = TimeSpan.FromSeconds(10);

    private Mock<IClock> _clock = null!;

    [SetUp]
    public void SetUp()
    {
        _clock = new Mock<IClock>();
        _clock.Setup(clock => clock.UtcNow).Returns(Now);
    }

    private RefreshToken NewToken()
        => RefreshToken.Create(TokenId, UserId, FamilyId, "hash-value", _clock.Object, Lifetime).Value;

    private void Advance(TimeSpan time) => _clock.Setup(clock => clock.UtcNow).Returns(Now + time);

    [Test]
    public void Create_WithValidData_KeepsTheFieldsAndComputesExpiry()
    {
        var result = RefreshToken.Create(TokenId, UserId, FamilyId, "hash-value", _clock.Object, Lifetime);

        Assert.That(result.IsSuccess, Is.True);
        var token = result.Value;
        Assert.That(token.Id, Is.EqualTo(TokenId));
        Assert.That(token.UserId, Is.EqualTo(UserId));
        Assert.That(token.FamilyId, Is.EqualTo(FamilyId));
        Assert.That(token.TokenHash, Is.EqualTo("hash-value"));
        Assert.That(token.CreatedAt, Is.EqualTo(Now));
        Assert.That(token.ExpiresAt, Is.EqualTo(Now + Lifetime));
        Assert.That(token.RevokedAt, Is.Null);
        Assert.That(token.ReplacedAt, Is.Null);
        Assert.That(token.ReplacedById, Is.Null);
    }

    [Test]
    public void Create_WithEmptyId_ReturnsIdRequired()
    {
        var result = RefreshToken.Create(Guid.Empty, UserId, FamilyId, "hash-value", _clock.Object, Lifetime);

        Assert.That(result.Error, Is.EqualTo(RefreshTokenErrors.IdRequired));
    }

    [Test]
    public void Create_WithEmptyUser_ReturnsUserRequired()
    {
        var result = RefreshToken.Create(TokenId, Guid.Empty, FamilyId, "hash-value", _clock.Object, Lifetime);

        Assert.That(result.Error, Is.EqualTo(RefreshTokenErrors.UserRequired));
    }

    [Test]
    public void Create_WithEmptyFamily_ReturnsFamilyRequired()
    {
        var result = RefreshToken.Create(TokenId, UserId, Guid.Empty, "hash-value", _clock.Object, Lifetime);

        Assert.That(result.Error, Is.EqualTo(RefreshTokenErrors.FamilyRequired));
    }

    [TestCase("")]
    [TestCase("   ")]
    public void Create_WithBlankHash_ReturnsHashRequired(string hash)
    {
        var result = RefreshToken.Create(TokenId, UserId, FamilyId, hash, _clock.Object, Lifetime);

        Assert.That(result.Error, Is.EqualTo(RefreshTokenErrors.HashRequired));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Create_WithNonPositiveLifetime_ReturnsLifetimeInvalid(int days)
    {
        var result = RefreshToken.Create(TokenId, UserId, FamilyId, "hash-value", _clock.Object, TimeSpan.FromDays(days));

        Assert.That(result.Error, Is.EqualTo(RefreshTokenErrors.LifetimeInvalid));
    }

    [Test]
    public void MarkReplaced_OnAnAlreadyReplacedToken_FailsAndKeepsTheFirstSuccessor()
    {
        var token = NewToken();
        token.MarkReplaced(SuccessorId, _clock.Object);

        var result = token.MarkReplaced(Guid.NewGuid(), _clock.Object);

        Assert.That(result.Error, Is.EqualTo(RefreshTokenErrors.NotActive));
        Assert.That(token.ReplacedById, Is.EqualTo(SuccessorId));
    }

    [Test]
    public void MarkReplaced_OnARevokedToken_Fails()
    {
        var token = NewToken();
        token.Revoke(_clock.Object);

        var result = token.MarkReplaced(SuccessorId, _clock.Object);

        Assert.That(result.Error, Is.EqualTo(RefreshTokenErrors.NotActive));
        Assert.That(token.ReplacedAt, Is.Null);
    }

    [Test]
    public void MarkReplaced_OnAnExpiredToken_Fails()
    {
        var token = NewToken();
        Advance(Lifetime);

        var result = token.MarkReplaced(SuccessorId, _clock.Object);

        Assert.That(result.Error, Is.EqualTo(RefreshTokenErrors.NotActive));
    }

    [Test]
    public void IsActive_ForAFreshToken_IsTrue()
    {
        Assert.That(NewToken().IsActive(_clock.Object), Is.True);
    }

    [Test]
    public void IsActive_AfterRevoke_IsFalse()
    {
        var token = NewToken();
        token.Revoke(_clock.Object);

        Assert.That(token.IsActive(_clock.Object), Is.False);
    }

    [Test]
    public void IsActive_AfterMarkReplaced_IsFalse()
    {
        var token = NewToken();
        token.MarkReplaced(SuccessorId, _clock.Object);

        Assert.That(token.IsActive(_clock.Object), Is.False);
    }

    [Test]
    public void IsActive_JustBeforeExpiry_IsTrue_AndAtExpiry_IsFalse()
    {
        var token = NewToken();

        Advance(Lifetime - TimeSpan.FromSeconds(1));
        Assert.That(token.IsActive(_clock.Object), Is.True);

        Advance(Lifetime);
        Assert.That(token.IsActive(_clock.Object), Is.False);
    }

    [Test]
    public void MarkReplaced_StampsTheDateAndTheSuccessor()
    {
        var token = NewToken();
        Advance(TimeSpan.FromMinutes(5));

        token.MarkReplaced(SuccessorId, _clock.Object);

        Assert.That(token.ReplacedAt, Is.EqualTo(Now + TimeSpan.FromMinutes(5)));
        Assert.That(token.ReplacedById, Is.EqualTo(SuccessorId));
    }

    [Test]
    public void Revoke_IsIdempotentAndKeepsTheFirstDate()
    {
        var token = NewToken();
        token.Revoke(_clock.Object);
        Advance(TimeSpan.FromHours(1));

        token.Revoke(_clock.Object);

        Assert.That(token.RevokedAt, Is.EqualTo(Now));
    }

    [Test]
    public void WasReplacedWithin_ForATokenNeverReplaced_IsFalse()
    {
        Assert.That(NewToken().WasReplacedWithin(_clock.Object, Grace), Is.False);
    }

    [Test]
    public void WasReplacedWithin_InsideTheGraceWindow_IsTrue()
    {
        var token = NewToken();
        token.MarkReplaced(SuccessorId, _clock.Object);
        Advance(TimeSpan.FromSeconds(3));

        Assert.That(token.WasReplacedWithin(_clock.Object, Grace), Is.True);
    }

    [Test]
    public void WasReplacedWithin_OutsideTheGraceWindow_IsFalse()
    {
        var token = NewToken();
        token.MarkReplaced(SuccessorId, _clock.Object);
        Advance(TimeSpan.FromSeconds(60));

        Assert.That(token.WasReplacedWithin(_clock.Object, Grace), Is.False);
    }

    [Test]
    public void Specification_FiltersByHashAndFamily_AndNullMeansNoFilter()
    {
        var token = NewToken();
        var other = RefreshToken.Create(Guid.NewGuid(), UserId, Guid.NewGuid(), "other-hash", _clock.Object, Lifetime).Value;
        var tokens = new[] { token, other };

        bool Matches(RefreshTokenSpecification spec, RefreshToken candidate) => spec.Criteria.Compile()(candidate);

        Assert.That(tokens.Count(t => Matches(new RefreshTokenSpecification(), t)), Is.EqualTo(2));
        Assert.That(tokens.Single(t => Matches(new RefreshTokenSpecification(tokenHash: "hash-value"), t)), Is.SameAs(token));
        Assert.That(tokens.Single(t => Matches(new RefreshTokenSpecification(familyId: FamilyId), t)), Is.SameAs(token));
        Assert.That(tokens.Count(t => Matches(new RefreshTokenSpecification("hash-value", other.FamilyId), t)), Is.EqualTo(0));
    }
}
