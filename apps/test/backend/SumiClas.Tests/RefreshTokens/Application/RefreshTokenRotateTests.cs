using Microsoft.Extensions.Options;
using Moq;
using SumiClas.RefreshTokens.Application;
using SumiClas.RefreshTokens.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;

namespace SumiClas.Tests.RefreshTokens.Application;

[TestFixture]
public class RefreshTokenRotateTests
{
    private static readonly Guid UserId = Guid.Parse("3f2b1c7e-8d4a-4c1e-9b5f-0a6d2e7c9b10");
    private static readonly DateTimeOffset Now = new(2026, 10, 10, 12, 0, 0, TimeSpan.Zero);

    private DateTimeOffset _now;
    private Mock<IClock> _clock = null!;
    private Mock<IQueryBus> _queryBus = null!;
    private Mock<IRefreshTokenGenerator> _generator = null!;
    private Mock<IRefreshTokenRepository> _repository = null!;
    private Mock<IAccessTokenGenerator> _accessTokenGenerator = null!;
    private List<RefreshToken> _stored = null!;
    private RefreshTokenRotate.Rotate _useCase = null!;

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
        _generator.Setup(generator => generator.Generate()).Returns(("new-raw", "hash-of-new-raw"));

        _repository = new Mock<IRefreshTokenRepository>();
        _repository
            .Setup(repository => repository.SearchAsync(It.IsAny<ISpecification<RefreshToken>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<RefreshToken> specification, CancellationToken _) =>
                (IReadOnlyCollection<RefreshToken>)_stored.Where(specification.Criteria.Compile()).ToList());

        _accessTokenGenerator = new Mock<IAccessTokenGenerator>();
        _accessTokenGenerator.Setup(generator => generator.Generate(UserId)).Returns(Result<AccessToken>.Success(AccessToken.Create("jwt", Now.AddMinutes(15)).Value));

        _useCase = new RefreshTokenRotate.Rotate(
            _repository.Object, _generator.Object, _accessTokenGenerator.Object, _queryBus.Object, _clock.Object,
            Options.Create(new JwtOptions { RefreshTokenDays = 30, RefreshGraceSeconds = 10 }));
    }

    private RefreshToken GivenAToken(string raw, Guid? familyId = null, int lifetimeDays = 30)
    {
        var token = RefreshToken.Create(
            Guid.NewGuid(), UserId, familyId ?? Guid.NewGuid(), "hash-of-" + raw, _clock.Object, TimeSpan.FromDays(lifetimeDays)).Value;
        _stored.Add(token);
        return token;
    }

    private void TimePasses(TimeSpan span) => _now += span;

    private void AssertNothingAdded()
        => _repository.Verify(repository => repository.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);

    [Test]
    public async Task UseCase_WhenTheIssuerFails_PropagatesTheFailure()
    {
        GivenAToken("old-raw");
        _accessTokenGenerator.Setup(generator => generator.Generate(UserId)).Returns(Result<AccessToken>.Failure(AccessTokenErrors.ValueRequired));

        var result = await _useCase.ExecuteAsync(new RefreshTokenRotate.Command("old-raw"));

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(AccessTokenErrors.ValueRequired));
    }

    [Test]
    public async Task UseCase_WithAnActiveToken_RotatesItAndReturnsTheNewSession()
    {
        var old = GivenAToken("old-raw");
        RefreshToken? successor = null;
        _repository
            .Setup(repository => repository.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Callback<RefreshToken, CancellationToken>((token, _) => successor = token)
            .Returns(Task.CompletedTask);

        var result = await _useCase.ExecuteAsync(new RefreshTokenRotate.Command("old-raw"));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(new RefreshOutcome.Rotated(new SessionResponse("jwt", Now.AddMinutes(15), UserId, "Ana Perez", "new-raw"))));
        Assert.That(successor, Is.Not.Null);
        Assert.That(successor!.FamilyId, Is.EqualTo(old.FamilyId));
        Assert.That(successor.TokenHash, Is.EqualTo("hash-of-new-raw"));
        Assert.That(successor.ExpiresAt, Is.EqualTo(Now.AddDays(30)));
        Assert.That(old.ReplacedById, Is.EqualTo(successor.Id));
        Assert.That(old.ReplacedAt, Is.EqualTo(Now));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task UseCase_WithABlankToken_IsInvalidAndSavesNothing(string? raw)
    {
        var result = await _useCase.ExecuteAsync(new RefreshTokenRotate.Command(raw));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.TypeOf<RefreshOutcome.Invalid>());
        AssertNothingAdded();
    }

    [Test]
    public async Task UseCase_WithAnUnknownToken_IsInvalidAndSavesNothing()
    {
        GivenAToken("other-raw");

        var result = await _useCase.ExecuteAsync(new RefreshTokenRotate.Command("unknown"));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.TypeOf<RefreshOutcome.Invalid>());
        AssertNothingAdded();
    }

    [Test]
    public async Task UseCase_WithAnExpiredToken_IsInvalidAndSavesNothing()
    {
        var token = GivenAToken("old-raw", lifetimeDays: 1);
        TimePasses(TimeSpan.FromDays(2));

        var result = await _useCase.ExecuteAsync(new RefreshTokenRotate.Command("old-raw"));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.TypeOf<RefreshOutcome.Invalid>());
        Assert.That(token.ReplacedAt, Is.Null);
        AssertNothingAdded();
    }

    [Test]
    public async Task UseCase_WithARevokedToken_IsInvalidAndSavesNothing()
    {
        var token = GivenAToken("old-raw");
        token.Revoke(_clock.Object);

        var result = await _useCase.ExecuteAsync(new RefreshTokenRotate.Command("old-raw"));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.TypeOf<RefreshOutcome.Invalid>());
        Assert.That(token.ReplacedAt, Is.Null);
        AssertNothingAdded();
    }

    [Test]
    public async Task UseCase_WithAReplacedTokenAfterTheGrace_RevokesTheWholeFamilyAndReportsReuseAsASuccess()
    {
        var family = Guid.NewGuid();
        var replaced = GivenAToken("old-raw", family);
        var successor = GivenAToken("successor-raw", family);
        var unrelated = GivenAToken("unrelated-raw");
        replaced.MarkReplaced(successor.Id, _clock.Object);
        TimePasses(TimeSpan.FromSeconds(11));

        var result = await _useCase.ExecuteAsync(new RefreshTokenRotate.Command("old-raw"));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.TypeOf<RefreshOutcome.Reused>());
        Assert.That(replaced.RevokedAt, Is.Not.Null);
        Assert.That(successor.RevokedAt, Is.Not.Null);
        Assert.That(unrelated.RevokedAt, Is.Null);
        AssertNothingAdded();
    }

    [Test]
    public async Task UseCase_WithAReplacedTokenWithinTheGrace_IsInvalidAndDoesNotRevokeTheFamily()
    {
        var family = Guid.NewGuid();
        var replaced = GivenAToken("old-raw", family);
        var successor = GivenAToken("successor-raw", family);
        replaced.MarkReplaced(successor.Id, _clock.Object);
        TimePasses(TimeSpan.FromSeconds(5));

        var result = await _useCase.ExecuteAsync(new RefreshTokenRotate.Command("old-raw"));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.TypeOf<RefreshOutcome.Invalid>());
        Assert.That(replaced.RevokedAt, Is.Null);
        Assert.That(successor.RevokedAt, Is.Null);
        AssertNothingAdded();
    }

    [Test]
    public async Task UseCase_WhenTheUserNoLongerExists_IsInvalidAndLeavesTheTokenUntouched()
    {
        var token = GivenAToken("old-raw");
        _queryBus
            .Setup(bus => bus.Send(It.IsAny<UserFinderById.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserResponse>.Failure(UserErrors.NotFound));

        var result = await _useCase.ExecuteAsync(new RefreshTokenRotate.Command("old-raw"));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.TypeOf<RefreshOutcome.Invalid>());
        Assert.That(token.ReplacedAt, Is.Null);
        AssertNothingAdded();
    }

    [Test]
    public async Task Handler_DelegatesToTheUseCase()
    {
        var handler = new RefreshTokenRotate.Handler(_useCase);

        var result = await handler.Handle(new RefreshTokenRotate.Command(null), CancellationToken.None);

        Assert.That(result.Value, Is.TypeOf<RefreshOutcome.Invalid>());
    }
}
