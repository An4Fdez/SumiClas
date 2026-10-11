using Moq;
using SumiClas.Sessions.Application;
using SumiClas.Sessions.Domain;
using SumiClas.Shared.Domain;

namespace SumiClas.Tests.Sessions.Application;

[TestFixture]
public class SessionRevokeTests
{
    private static readonly Guid UserId = Guid.Parse("3f2b1c7e-8d4a-4c1e-9b5f-0a6d2e7c9b10");
    private static readonly DateTimeOffset Now = new(2026, 10, 10, 12, 0, 0, TimeSpan.Zero);

    private DateTimeOffset _now;
    private Mock<IClock> _clock = null!;
    private List<Session> _stored = null!;
    private SessionRevoke.Revoke _useCase = null!;

    [SetUp]
    public void SetUp()
    {
        _now = Now;
        _clock = new Mock<IClock>();
        _clock.Setup(c => c.UtcNow).Returns(() => _now);
        _stored = [];

        var generator = new Mock<IRefreshTokenGenerator>();
        generator.Setup(g => g.Hash(It.IsAny<string>())).Returns<string>(raw => "hash-of-" + raw);

        var repository = new Mock<ISessionRepository>();
        repository
            .Setup(r => r.SearchAsync(It.IsAny<ISpecification<Session>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<Session> specification, CancellationToken _) =>
                (IReadOnlyCollection<Session>)_stored.Where(specification.Criteria.Compile()).ToList());

        _useCase = new SessionRevoke.Revoke(repository.Object, generator.Object, _clock.Object);
    }

    private Session GivenASession(string raw)
    {
        var session = Session.Create(UserId, "hash-of-" + raw, _clock.Object).Value;
        _stored.Add(session);
        return session;
    }

    [Test]
    public async Task UseCase_WithAKnownToken_RevokesItsSessionOnly()
    {
        var session = GivenASession("active-raw");
        var other = GivenASession("other-raw");

        var result = await _useCase.ExecuteAsync(new SessionRevoke.Command("active-raw"));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(session.RevokedAt, Is.EqualTo(Now));
        Assert.That(other.RevokedAt, Is.Null);
    }

    [Test]
    public async Task UseCase_WithATokenAlreadyReplaced_StillRevokesTheSession()
    {
        var session = GivenASession("old-raw");
        session.Rotate("hash-of-old-raw", "hash-of-new-raw", _clock.Object);

        var result = await _useCase.ExecuteAsync(new SessionRevoke.Command("old-raw"));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(session.RevokedAt, Is.Not.Null);
    }

    [Test]
    public async Task UseCase_CalledTwice_KeepsTheFirstRevocationAndSucceeds()
    {
        var session = GivenASession("raw");
        await _useCase.ExecuteAsync(new SessionRevoke.Command("raw"));
        var firstRevocation = session.RevokedAt;
        _now += TimeSpan.FromHours(1);

        var result = await _useCase.ExecuteAsync(new SessionRevoke.Command("raw"));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(session.RevokedAt, Is.EqualTo(firstRevocation));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("  ")]
    [TestCase("unknown")]
    public async Task UseCase_WithABlankOrUnknownToken_SucceedsAndTouchesNothing(string? raw)
    {
        var session = GivenASession("raw");

        var result = await _useCase.ExecuteAsync(new SessionRevoke.Command(raw));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(session.RevokedAt, Is.Null);
    }

    [Test]
    public async Task Handler_DelegatesToTheUseCase()
    {
        var handler = new SessionRevoke.Handler(_useCase);

        var result = await handler.Handle(new SessionRevoke.Command(null), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
    }
}
