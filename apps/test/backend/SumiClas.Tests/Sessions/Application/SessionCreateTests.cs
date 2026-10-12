using Moq;
using SumiClas.Sessions.Application;
using SumiClas.Sessions.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.Tests.Sessions.Application;

[TestFixture]
public class SessionCreateTests
{
    private static readonly Guid UserId = Guid.Parse("3f2b1c7e-8d4a-4c1e-9b5f-0a6d2e7c9b10");
    private static readonly DateTimeOffset Now = new(2026, 10, 10, 12, 0, 0, TimeSpan.Zero);

    private Mock<IQueryBus> _queryBus = null!;
    private Mock<IClock> _clock = null!;
    private Mock<IRefreshTokenGenerator> _generator = null!;
    private Mock<ISessionRepository> _sessions = null!;
    private Mock<IAccessTokenGenerator> _accessTokenGenerator = null!;
    private SessionCreate.Create _useCase = null!;

    [SetUp]
    public void SetUp()
    {
        _clock = new Mock<IClock>();
        _clock.Setup(c => c.UtcNow).Returns(Now);

        _queryBus = new Mock<IQueryBus>();
        _queryBus
            .Setup(bus => bus.Send(It.IsAny<UserPasswordVerify.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserResponse>.Success(new UserResponse(UserId, "Ana Perez")));

        _generator = new Mock<IRefreshTokenGenerator>();
        _generator.Setup(generator => generator.Generate()).Returns("raw-refresh");
        _generator.Setup(generator => generator.Hash(It.IsAny<string>())).Returns<string>(raw => "hash-of-" + raw);
        _sessions = new Mock<ISessionRepository>();
        _accessTokenGenerator = new Mock<IAccessTokenGenerator>();
        _accessTokenGenerator
            .Setup(generator => generator.Generate(UserId))
            .Returns(Result<AccessToken>.Success(AccessToken.Create("jwt", Now + AccessToken.Lifetime).Value));

        _useCase = new SessionCreate.Create(
            _queryBus.Object, _generator.Object, _sessions.Object, _accessTokenGenerator.Object, _clock.Object);
    }

    [Test]
    public async Task UseCase_WithValidCredentials_ReturnsTheSessionAndStoresOnlyTheHashInANewSession()
    {
        var result = await _useCase.ExecuteAsync(new SessionCreate.Command("ana@example.com", "right-password"));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(new SessionResponse("jwt", Now + AccessToken.Lifetime, UserId, "Ana Perez", "raw-refresh", Now + RefreshToken.Lifetime)));
        _queryBus.Verify(bus => bus.Send(
            It.Is<UserPasswordVerify.Query>(query => query.Email == "ana@example.com" && query.Password == "right-password"),
            It.IsAny<CancellationToken>()), Times.Once);
        _sessions.Verify(repository => repository.AddAsync(
            It.Is<Session>(session =>
                session.UserId == UserId
                && session.Id != Guid.Empty
                && session.RevokedAt == null
                && session.RefreshTokens.Count == 1
                && session.RefreshTokens.Single().Id != Guid.Empty
                && session.RefreshTokens.Single().Id != session.Id
                && session.RefreshTokens.Single().TokenHash == "hash-of-raw-refresh"
                && session.RefreshTokens.Single().ExpiresAt == Now + RefreshToken.Lifetime),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UseCase_WhenTheCredentialCheckFails_PropagatesTheFailureAndCreatesNothing()
    {
        _queryBus
            .Setup(bus => bus.Send(It.IsAny<UserPasswordVerify.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserResponse>.Failure(AuthenticationErrors.InvalidCredentials));

        var result = await _useCase.ExecuteAsync(new SessionCreate.Command("ana@example.com", "wrong"));

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(AuthenticationErrors.InvalidCredentials));
        _generator.Verify(generator => generator.Generate(), Times.Never);
        _sessions.Verify(repository => repository.AddAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()), Times.Never);
        _accessTokenGenerator.Verify(generator => generator.Generate(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task UseCase_WhenTheIssuerFails_PropagatesTheFailure()
    {
        _accessTokenGenerator.Setup(generator => generator.Generate(UserId)).Returns(Result<AccessToken>.Failure(AccessTokenErrors.ValueRequired));

        var result = await _useCase.ExecuteAsync(new SessionCreate.Command("ana@example.com", "right-password"));

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(AccessTokenErrors.ValueRequired));
    }

    [Test]
    public async Task Handler_DelegatesToTheUseCase()
    {
        var handler = new SessionCreate.Handler(_useCase);

        var result = await handler.Handle(new SessionCreate.Command("ana@example.com", "right-password"), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
    }
}
