using Microsoft.Extensions.Options;
using Moq;
using SumiClas.Sessions.Application;
using SumiClas.Sessions.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

namespace SumiClas.Tests.Sessions.Application;

[TestFixture]
public class SessionRotateTests
{
    private static readonly Guid UserId = Guid.Parse("3f2b1c7e-8d4a-4c1e-9b5f-0a6d2e7c9b10");
    private static readonly DateTimeOffset Now = new(2026, 10, 10, 12, 0, 0, TimeSpan.Zero);

    private DateTimeOffset _now;
    private Mock<IClock> _clock = null!;
    private Mock<IQueryBus> _queryBus = null!;
    private Mock<IRefreshTokenGenerator> _generator = null!;
    private Mock<ISessionRepository> _repository = null!;
    private Mock<IAccessTokenGenerator> _accessTokenGenerator = null!;
    private List<Session> _stored = null!;
    private SessionRotate.Rotate _useCase = null!;

    [SetUp]
    public void SetUp()
    {
        _now = Now;
        _clock = new Mock<IClock>();
        _clock.Setup(c => c.UtcNow).Returns(() => _now);
        _stored = [];

        _queryBus = new Mock<IQueryBus>();
        _queryBus
            .Setup(bus => bus.Send(It.IsAny<UserFinderById.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserResponse>.Success(new UserResponse(UserId, "Ana Perez")));

        _generator = new Mock<IRefreshTokenGenerator>();
        _generator.Setup(generator => generator.Hash(It.IsAny<string>())).Returns<string>(raw => "hash-of-" + raw);
        _generator.Setup(generator => generator.Generate()).Returns("new-raw");

        _repository = new Mock<ISessionRepository>();
        _repository
            .Setup(repository => repository.SearchAsync(It.IsAny<ISpecification<Session>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<Session> specification, CancellationToken _) =>
                (IReadOnlyCollection<Session>)_stored.Where(specification.Criteria.Compile()).ToList());

        _accessTokenGenerator = new Mock<IAccessTokenGenerator>();
        _accessTokenGenerator.Setup(generator => generator.Generate(UserId)).Returns(Result<AccessToken>.Success(AccessToken.Create("jwt", Now.AddMinutes(15)).Value));

        _useCase = new SessionRotate.Rotate(
            _repository.Object, _generator.Object, _accessTokenGenerator.Object, _queryBus.Object, _clock.Object,
            Options.Create(new JwtOptions { RefreshTokenDays = 30, RefreshGraceSeconds = 10 }));
    }

    private Session GivenASession(string raw, int lifetimeDays = 30)
    {
        var session = Session.Create(
            UserId, "hash-of-" + raw, _clock.Object, TimeSpan.FromDays(lifetimeDays)).Value;
        _stored.Add(session);
        return session;
    }

    private void TimePasses(TimeSpan span) => _now += span;

    private static RefreshToken TokenOf(Session session, string raw) => session.RefreshTokens.Single(token => token.TokenHash == "hash-of-" + raw);

    [Test]
    public async Task UseCase_WhenTheIssuerFails_PropagatesTheFailure()
    {
        GivenASession("old-raw");
        _accessTokenGenerator.Setup(generator => generator.Generate(UserId)).Returns(Result<AccessToken>.Failure(AccessTokenErrors.ValueRequired));

        var result = await _useCase.ExecuteAsync(new SessionRotate.Command("old-raw"));

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(AccessTokenErrors.ValueRequired));
    }

    [Test]
    public async Task UseCase_WithAnActiveToken_RotatesItAndReturnsTheNewSession()
    {
        var session = GivenASession("old-raw");

        var result = await _useCase.ExecuteAsync(new SessionRotate.Command("old-raw"));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(new SessionRotateOutcome.Rotated(new SessionResponse("jwt", Now.AddMinutes(15), UserId, "Ana Perez", "new-raw"))));
        var old = TokenOf(session, "old-raw");
        var successor = TokenOf(session, "new-raw");
        Assert.That(session.RefreshTokens, Has.Count.EqualTo(2));
        Assert.That(successor.TokenHash, Is.EqualTo("hash-of-new-raw"));
        Assert.That(successor.ExpiresAt, Is.EqualTo(Now.AddDays(30)));
        Assert.That(successor.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(successor.Id, Is.Not.EqualTo(session.Id));
        Assert.That(successor.Id, Is.Not.EqualTo(old.Id));
        Assert.That(old.ReplacedById, Is.EqualTo(successor.Id));
        Assert.That(old.ReplacedAt, Is.EqualTo(Now));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task UseCase_WithABlankToken_IsInvalidAndSearchesNothing(string? raw)
    {
        var result = await _useCase.ExecuteAsync(new SessionRotate.Command(raw));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.TypeOf<SessionRotateOutcome.Invalid>());
        _repository.Verify(repository => repository.SearchAsync(It.IsAny<ISpecification<Session>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task UseCase_WithAnUnknownToken_IsInvalidAndChangesNothing()
    {
        var session = GivenASession("other-raw");

        var result = await _useCase.ExecuteAsync(new SessionRotate.Command("unknown"));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.TypeOf<SessionRotateOutcome.Invalid>());
        Assert.That(session.RefreshTokens, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task UseCase_WithAnExpiredToken_IsInvalidAndChangesNothing()
    {
        var session = GivenASession("old-raw", lifetimeDays: 1);
        TimePasses(TimeSpan.FromDays(2));

        var result = await _useCase.ExecuteAsync(new SessionRotate.Command("old-raw"));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.TypeOf<SessionRotateOutcome.Invalid>());
        Assert.That(TokenOf(session, "old-raw").ReplacedAt, Is.Null);
        Assert.That(session.RefreshTokens, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task UseCase_WithARevokedSession_IsInvalidAndChangesNothing()
    {
        var session = GivenASession("old-raw");
        session.Revoke(_clock.Object);

        var result = await _useCase.ExecuteAsync(new SessionRotate.Command("old-raw"));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.TypeOf<SessionRotateOutcome.Invalid>());
        Assert.That(TokenOf(session, "old-raw").ReplacedAt, Is.Null);
        Assert.That(session.RefreshTokens, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task UseCase_WithAReplacedTokenAfterTheGrace_RevokesTheSessionAndReportsReuseAsASuccess()
    {
        var session = GivenASession("old-raw");
        var unrelated = GivenASession("unrelated-raw");
        await _useCase.ExecuteAsync(new SessionRotate.Command("old-raw"));
        TimePasses(TimeSpan.FromSeconds(11));

        var result = await _useCase.ExecuteAsync(new SessionRotate.Command("old-raw"));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.TypeOf<SessionRotateOutcome.Reused>());
        Assert.That(session.RevokedAt, Is.Not.Null);
        Assert.That(unrelated.RevokedAt, Is.Null);
        Assert.That(session.RefreshTokens, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task UseCase_WithAReplacedTokenWithinTheGrace_IsInvalidAndDoesNotRevokeTheSession()
    {
        var session = GivenASession("old-raw");
        await _useCase.ExecuteAsync(new SessionRotate.Command("old-raw"));
        TimePasses(TimeSpan.FromSeconds(5));

        var result = await _useCase.ExecuteAsync(new SessionRotate.Command("old-raw"));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.TypeOf<SessionRotateOutcome.Invalid>());
        Assert.That(session.RevokedAt, Is.Null);
        Assert.That(session.RefreshTokens, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task UseCase_WhenTheUserNoLongerExists_IsInvalidAndLeavesTheSessionUntouched()
    {
        var session = GivenASession("old-raw");
        _queryBus
            .Setup(bus => bus.Send(It.IsAny<UserFinderById.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserResponse>.Failure(UserErrors.NotFound));

        var result = await _useCase.ExecuteAsync(new SessionRotate.Command("old-raw"));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.TypeOf<SessionRotateOutcome.Invalid>());
        Assert.That(TokenOf(session, "old-raw").ReplacedAt, Is.Null);
        Assert.That(session.RefreshTokens, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task Handler_DelegatesToTheUseCase()
    {
        var handler = new SessionRotate.Handler(_useCase);

        var result = await handler.Handle(new SessionRotate.Command(null), CancellationToken.None);

        Assert.That(result.Value, Is.TypeOf<SessionRotateOutcome.Invalid>());
    }
}
