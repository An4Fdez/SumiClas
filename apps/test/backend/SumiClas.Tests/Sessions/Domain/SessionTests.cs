using Moq;
using SumiClas.Sessions.Domain;
using SumiClas.Shared.Domain;

namespace SumiClas.Tests.Sessions.Domain;

[TestFixture]
public class SessionTests
{
    private static readonly Guid UserId = Guid.Parse("3f2b1c7e-8d4a-4c1e-9b5f-0a6d2e7c9b10");
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

    private Session NewSession(TimeSpan? lifetime = null)
        => Session.Create(UserId, "hash-1", _clock.Object, lifetime ?? Lifetime).Value;

    private void Advance(TimeSpan time) => _clock.Setup(clock => clock.UtcNow).Returns(Now + time);

    private static RefreshToken TokenOf(Session session, string hash) => session.RefreshTokens.Single(token => token.TokenHash == hash);

    private Result<SessionRotation> Rotate(Session session, string hash, string newHash, TimeSpan? lifetime = null)
        => session.Rotate(hash, newHash, _clock.Object, lifetime ?? Lifetime, Grace);

    [Test]
    public void Start_WithValidData_KeepsTheFieldsAndCreatesTheFirstToken()
    {
        var result = Session.Create(UserId, "hash-1", _clock.Object, Lifetime);

        Assert.That(result.IsSuccess, Is.True);
        var session = result.Value;
        Assert.That(session.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(session.UserId, Is.EqualTo(UserId));
        Assert.That(session.CreatedAt, Is.EqualTo(Now));
        Assert.That(session.RevokedAt, Is.Null);
        var token = session.RefreshTokens.Single();
        Assert.That(token.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(token.Id, Is.Not.EqualTo(session.Id));
        Assert.That(token.TokenHash, Is.EqualTo("hash-1"));
        Assert.That(token.CreatedAt, Is.EqualTo(Now));
        Assert.That(token.ExpiresAt, Is.EqualTo(Now + Lifetime));
        Assert.That(token.ReplacedAt, Is.Null);
        Assert.That(token.ReplacedById, Is.Null);
    }

    [Test]
    public void Start_AssignsADifferentIdOnEachCall()
    {
        var first = Session.Create(UserId, "hash-1", _clock.Object, Lifetime).Value;
        var second = Session.Create(UserId, "hash-1", _clock.Object, Lifetime).Value;

        Assert.That(first.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(second.Id, Is.Not.EqualTo(first.Id));
    }

    [Test]
    public void Start_WithEmptyUser_ReturnsUserRequired()
    {
        var result = Session.Create(Guid.Empty, "hash-1", _clock.Object, Lifetime);

        Assert.That(result.Error, Is.EqualTo(SessionErrors.UserRequired));
    }

    [TestCase("")]
    [TestCase("   ")]
    public void Start_WithBlankHash_ReturnsTokenHashRequired(string hash)
    {
        var result = Session.Create(UserId, hash, _clock.Object, Lifetime);

        Assert.That(result.Error, Is.EqualTo(SessionErrors.TokenHashRequired));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Start_WithNonPositiveLifetime_ReturnsLifetimeInvalid(int days)
    {
        var result = Session.Create(UserId, "hash-1", _clock.Object, TimeSpan.FromDays(days));

        Assert.That(result.Error, Is.EqualTo(SessionErrors.LifetimeInvalid));
    }

    [Test]
    public void Rotate_WithTheActiveToken_ReplacesItAndAddsTheSuccessor()
    {
        var session = NewSession();
        var firstId = session.RefreshTokens.Single().Id;
        Advance(TimeSpan.FromMinutes(5));

        var result = Rotate(session, "hash-1", "hash-2");

        Assert.That(result.IsSuccess, Is.True);
        var rotated = result.Value as SessionRotation.Rotated;
        Assert.That(rotated, Is.Not.Null);
        Assert.That(rotated!.NewToken.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(rotated.NewToken.Id, Is.Not.EqualTo(firstId));
        Assert.That(rotated.NewToken.Id, Is.Not.EqualTo(session.Id));
        Assert.That(rotated.NewToken.TokenHash, Is.EqualTo("hash-2"));
        Assert.That(rotated.NewToken.ExpiresAt, Is.EqualTo(Now + TimeSpan.FromMinutes(5) + Lifetime));
        var old = session.RefreshTokens.Single(token => token.Id == firstId);
        Assert.That(old.ReplacedAt, Is.EqualTo(Now + TimeSpan.FromMinutes(5)));
        Assert.That(old.ReplacedById, Is.EqualTo(rotated.NewToken.Id));
        Assert.That(session.RefreshTokens, Has.Count.EqualTo(2));
        Assert.That(session.RevokedAt, Is.Null);
    }

    [TestCase("", "hash-2")]
    [TestCase("hash-1", " ")]
    public void Rotate_WithABlankHash_ReturnsTokenHashRequired(string hash, string newHash)
    {
        var session = NewSession();

        var result = Rotate(session, hash, newHash);

        Assert.That(result.Error, Is.EqualTo(SessionErrors.TokenHashRequired));
        Assert.That(session.RefreshTokens, Has.Count.EqualTo(1));
    }

    [Test]
    public void Rotate_WithANonPositiveLifetime_ReturnsLifetimeInvalid()
    {
        var session = NewSession();

        var result = Rotate(session, "hash-1", "hash-2", TimeSpan.Zero);

        Assert.That(result.Error, Is.EqualTo(SessionErrors.LifetimeInvalid));
        Assert.That(session.RefreshTokens, Has.Count.EqualTo(1));
    }

    [Test]
    public void Rotate_WithAnUnknownHash_IsInvalidAndChangesNothing()
    {
        var session = NewSession();

        var result = Rotate(session, "unknown", "hash-2");

        Assert.That(result.Value, Is.TypeOf<SessionRotation.Invalid>());
        Assert.That(session.RefreshTokens, Has.Count.EqualTo(1));
        Assert.That(session.RefreshTokens.Single().ReplacedAt, Is.Null);
        Assert.That(session.RevokedAt, Is.Null);
    }

    [Test]
    public void Rotate_WithAnExpiredToken_IsInvalidAndChangesNothing()
    {
        var session = NewSession(TimeSpan.FromDays(1));
        Advance(TimeSpan.FromDays(2));

        var result = Rotate(session, "hash-1", "hash-2");

        Assert.That(result.Value, Is.TypeOf<SessionRotation.Invalid>());
        Assert.That(session.RefreshTokens, Has.Count.EqualTo(1));
        Assert.That(session.RefreshTokens.Single().ReplacedAt, Is.Null);
        Assert.That(session.RevokedAt, Is.Null);
    }

    [Test]
    public void Rotate_WithATokenReplacedWithinTheGrace_IsInvalidAndChangesNothing()
    {
        var session = NewSession();
        Rotate(session, "hash-1", "hash-2");
        Advance(TimeSpan.FromSeconds(5));

        var result = Rotate(session, "hash-1", "hash-3");

        Assert.That(result.Value, Is.TypeOf<SessionRotation.Invalid>());
        Assert.That(session.RevokedAt, Is.Null);
        Assert.That(session.RefreshTokens, Has.Count.EqualTo(2));
        Assert.That(session.RefreshTokens.Single(token => token.TokenHash == "hash-2").IsActive(_clock.Object), Is.True);
    }

    [Test]
    public void Rotate_WithATokenReplacedExactlyAtTheGraceLimit_IsStillInvalidWithoutRevoking()
    {
        var session = NewSession();
        Rotate(session, "hash-1", "hash-2");
        Advance(Grace);

        var result = Rotate(session, "hash-1", "hash-3");

        Assert.That(result.Value, Is.TypeOf<SessionRotation.Invalid>());
        Assert.That(session.RevokedAt, Is.Null);
    }

    [Test]
    public void Rotate_WithATokenReplacedOutsideTheGrace_RevokesTheSessionAndReportsReuse()
    {
        var session = NewSession();
        Rotate(session, "hash-1", "hash-2");
        Advance(TimeSpan.FromSeconds(60));

        var result = Rotate(session, "hash-1", "hash-3");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.TypeOf<SessionRotation.Reused>());
        Assert.That(session.RevokedAt, Is.EqualTo(Now + TimeSpan.FromSeconds(60)));
        Assert.That(session.IsActive(_clock.Object), Is.False);
        Assert.That(session.RefreshTokens, Has.Count.EqualTo(2));
    }

    [Test]
    public void Rotate_AfterTheSessionWasRevokedByReuse_RejectsTheSuccessor()
    {
        var session = NewSession();
        Rotate(session, "hash-1", "hash-2");
        Advance(TimeSpan.FromSeconds(60));
        Rotate(session, "hash-1", "hash-3");

        var result = Rotate(session, "hash-2", "hash-4");

        Assert.That(result.Value, Is.TypeOf<SessionRotation.Invalid>());
        Assert.That(session.RefreshTokens, Has.Count.EqualTo(2));
    }

    [Test]
    public void Rotate_OnARevokedSession_IsInvalidAndChangesNothing()
    {
        var session = NewSession();
        session.Revoke(_clock.Object);

        var result = Rotate(session, "hash-1", "hash-2");

        Assert.That(result.Value, Is.TypeOf<SessionRotation.Invalid>());
        Assert.That(session.RefreshTokens, Has.Count.EqualTo(1));
        Assert.That(session.RefreshTokens.Single().ReplacedAt, Is.Null);
    }

    [Test]
    public void Rotate_PrunesTokensReplacedLongAgo_ButKeepsTheCurrentAndTheJustReplacedOnes()
    {
        var session = NewSession();
        Advance(TimeSpan.FromHours(1));
        Rotate(session, "hash-1", "hash-2");
        Advance(TimeSpan.FromHours(26));

        var result = Rotate(session, "hash-2", "hash-3");

        Assert.That(result.Value, Is.TypeOf<SessionRotation.Rotated>());
        Assert.That(session.RefreshTokens.Select(token => token.Id), Is.EquivalentTo(new[] { TokenOf(session, "hash-2").Id, TokenOf(session, "hash-3").Id }));
    }

    [Test]
    public void Rotate_KeepsTokensReplacedWithinTheRetention()
    {
        var session = NewSession();
        Advance(TimeSpan.FromHours(1));
        Rotate(session, "hash-1", "hash-2");
        Advance(TimeSpan.FromHours(1) + Session.ReplacedTokenRetention - TimeSpan.FromHours(1));

        Rotate(session, "hash-2", "hash-3");

        Assert.That(session.RefreshTokens, Has.Count.EqualTo(3));
    }

    [Test]
    public void Rotate_PrunesExpiredTokens()
    {
        var session = NewSession(TimeSpan.FromHours(1));
        Advance(TimeSpan.FromMinutes(30));
        Rotate(session, "hash-1", "hash-2");
        Advance(TimeSpan.FromMinutes(90));

        var result = Rotate(session, "hash-2", "hash-3");

        Assert.That(result.Value, Is.TypeOf<SessionRotation.Rotated>());
        Assert.That(session.RefreshTokens.Select(token => token.Id), Is.EquivalentTo(new[] { TokenOf(session, "hash-2").Id, TokenOf(session, "hash-3").Id }));
    }

    [Test]
    public void Rotate_AfterPruning_ReplayOfAnOldToken_IsInvalidInsteadOfReused()
    {
        var session = NewSession();
        Advance(TimeSpan.FromHours(1));
        Rotate(session, "hash-1", "hash-2");
        Advance(TimeSpan.FromHours(26));
        Rotate(session, "hash-2", "hash-3");

        var result = Rotate(session, "hash-1", "hash-4");

        Assert.That(result.Value, Is.TypeOf<SessionRotation.Invalid>());
        Assert.That(session.RevokedAt, Is.Null);
    }

    [Test]
    public void Revoke_IsIdempotentAndKeepsTheFirstDate()
    {
        var session = NewSession();
        session.Revoke(_clock.Object);
        Advance(TimeSpan.FromHours(1));

        session.Revoke(_clock.Object);

        Assert.That(session.RevokedAt, Is.EqualTo(Now));
    }

    [Test]
    public void IsActive_ForAFreshSession_IsTrue_AndAfterRevoke_IsFalse()
    {
        var session = NewSession();
        Assert.That(session.IsActive(_clock.Object), Is.True);

        session.Revoke(_clock.Object);

        Assert.That(session.IsActive(_clock.Object), Is.False);
    }

    [Test]
    public void HasToken_MatchesOnlyTheHashesOfItsTokens()
    {
        var session = NewSession();

        Assert.That(session.HasToken("hash-1"), Is.True);
        Assert.That(session.HasToken("other"), Is.False);
    }

    [Test]
    public void RefreshToken_IsActive_JustBeforeExpiry_IsTrue_AndAtExpiry_IsFalse()
    {
        var token = NewSession().RefreshTokens.Single();

        Advance(Lifetime - TimeSpan.FromSeconds(1));
        Assert.That(token.IsActive(_clock.Object), Is.True);

        Advance(Lifetime);
        Assert.That(token.IsActive(_clock.Object), Is.False);
    }

    [Test]
    public void RefreshToken_IsActive_AfterBeingReplaced_IsFalse()
    {
        var session = NewSession();
        Rotate(session, "hash-1", "hash-2");

        Assert.That(TokenOf(session, "hash-1").IsActive(_clock.Object), Is.False);
    }

    [Test]
    public void RefreshToken_WasReplacedWithin_ForATokenNeverReplaced_IsFalse()
    {
        Assert.That(NewSession().RefreshTokens.Single().WasReplacedWithin(_clock.Object, Grace), Is.False);
    }

    [Test]
    public void RefreshToken_WasReplacedWithin_DependsOnTheGraceWindow()
    {
        var session = NewSession();
        Rotate(session, "hash-1", "hash-2");
        var old = TokenOf(session, "hash-1");

        Advance(TimeSpan.FromSeconds(3));
        Assert.That(old.WasReplacedWithin(_clock.Object, Grace), Is.True);

        Advance(TimeSpan.FromSeconds(60));
        Assert.That(old.WasReplacedWithin(_clock.Object, Grace), Is.False);
    }

    [Test]
    public void Specification_FiltersByIdUserAndTokenHash_AndNullMeansNoFilter()
    {
        var session = NewSession();
        var other = Session.Create(Guid.NewGuid(), "other-hash", _clock.Object, Lifetime).Value;
        Rotate(session, "hash-1", "hash-2");
        var sessions = new[] { session, other };

        bool Matches(SessionSpecification spec, Session candidate) => spec.Criteria.Compile()(candidate);

        Assert.That(sessions.Count(s => Matches(new SessionSpecification(), s)), Is.EqualTo(2));
        Assert.That(sessions.Single(s => Matches(new SessionSpecification(id: session.Id), s)), Is.SameAs(session));
        Assert.That(sessions.Single(s => Matches(new SessionSpecification(userId: UserId), s)), Is.SameAs(session));
        Assert.That(sessions.Single(s => Matches(new SessionSpecification(tokenHash: "hash-2"), s)), Is.SameAs(session));
        Assert.That(sessions.Single(s => Matches(new SessionSpecification(tokenHash: "other-hash"), s)), Is.SameAs(other));
        Assert.That(sessions.Count(s => Matches(new SessionSpecification(id: session.Id, tokenHash: "other-hash"), s)), Is.EqualTo(0));
    }
}
