using Moq;
using SumiClas.RefreshTokens.Application;
using SumiClas.RefreshTokens.Domain;
using SumiClas.Shared.Domain;

namespace SumiClas.Tests.RefreshTokens.Application;

[TestFixture]
public class RefreshTokenRevokeTests
{
    private static readonly Guid UserId = Guid.Parse("3f2b1c7e-8d4a-4c1e-9b5f-0a6d2e7c9b10");
    private static readonly DateTimeOffset Now = new(2026, 10, 10, 12, 0, 0, TimeSpan.Zero);

    private DateTimeOffset _now;
    private Mock<IClock> _clock = null!;
    private List<RefreshToken> _stored = null!;
    private RefreshTokenRevoke.Revoke _useCase = null!;

    [SetUp]
    public void SetUp()
    {
        _now = Now;
        _clock = new Mock<IClock>();
        _clock.Setup(c => c.UtcNow).Returns(() => _now);
        _stored = [];

        var generator = new Mock<IRefreshTokenGenerator>();
        generator.Setup(g => g.Hash(It.IsAny<string>())).Returns<string>(raw => "hash-of-" + raw);

        var repository = new Mock<IRefreshTokenRepository>();
        repository
            .Setup(r => r.SearchAsync(It.IsAny<ISpecification<RefreshToken>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<RefreshToken> specification, CancellationToken _) =>
                (IReadOnlyCollection<RefreshToken>)_stored.Where(specification.Criteria.Compile()).ToList());

        _useCase = new RefreshTokenRevoke.Revoke(repository.Object, generator.Object, _clock.Object);
    }

    private RefreshToken GivenAToken(string raw, Guid family)
    {
        var token = RefreshToken.Create(Guid.NewGuid(), UserId, family, "hash-of-" + raw, _clock.Object, TimeSpan.FromDays(30)).Value;
        _stored.Add(token);
        return token;
    }

    [Test]
    public async Task UseCase_WithAKnownToken_RevokesTheWholeFamily()
    {
        var family = Guid.NewGuid();
        var replaced = GivenAToken("old-raw", family);
        var active = GivenAToken("active-raw", family);
        var other = GivenAToken("other-raw", Guid.NewGuid());
        replaced.MarkReplaced(active.Id, _clock.Object);

        var result = await _useCase.ExecuteAsync(new RefreshTokenRevoke.Command("active-raw"));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(replaced.RevokedAt, Is.Not.Null);
        Assert.That(active.RevokedAt, Is.Not.Null);
        Assert.That(other.RevokedAt, Is.Null);
    }

    [Test]
    public async Task UseCase_CalledTwice_KeepsTheFirstRevocationAndSucceeds()
    {
        var token = GivenAToken("raw", Guid.NewGuid());
        await _useCase.ExecuteAsync(new RefreshTokenRevoke.Command("raw"));
        var firstRevocation = token.RevokedAt;
        _now += TimeSpan.FromHours(1);

        var result = await _useCase.ExecuteAsync(new RefreshTokenRevoke.Command("raw"));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(token.RevokedAt, Is.EqualTo(firstRevocation));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("  ")]
    [TestCase("unknown")]
    public async Task UseCase_WithABlankOrUnknownToken_SucceedsAndTouchesNothing(string? raw)
    {
        var token = GivenAToken("raw", Guid.NewGuid());

        var result = await _useCase.ExecuteAsync(new RefreshTokenRevoke.Command(raw));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(token.RevokedAt, Is.Null);
    }

    [Test]
    public async Task Handler_DelegatesToTheUseCase()
    {
        var handler = new RefreshTokenRevoke.Handler(_useCase);

        var result = await handler.Handle(new RefreshTokenRevoke.Command(null), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
    }
}
