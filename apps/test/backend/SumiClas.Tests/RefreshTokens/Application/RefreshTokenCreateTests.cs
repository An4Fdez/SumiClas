using Microsoft.Extensions.Options;
using Moq;
using SumiClas.RefreshTokens.Application;
using SumiClas.RefreshTokens.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.Tests.RefreshTokens.Application;

[TestFixture]
public class RefreshTokenCreateTests
{
    private static readonly Guid UserId = Guid.Parse("3f2b1c7e-8d4a-4c1e-9b5f-0a6d2e7c9b10");
    private static readonly DateTimeOffset Now = new(2026, 10, 10, 12, 0, 0, TimeSpan.Zero);

    private Mock<IQueryBus> _queryBus = null!;
    private Mock<IClock> _clock = null!;
    private Mock<IRefreshTokenGenerator> _generator = null!;
    private Mock<IRefreshTokenRepository> _refreshTokens = null!;
    private Mock<IAccessTokenGenerator> _accessTokenGenerator = null!;
    private RefreshTokenCreate.Create _useCase = null!;

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
        _generator.Setup(generator => generator.Generate()).Returns(("raw-refresh", "hash-refresh"));
        _refreshTokens = new Mock<IRefreshTokenRepository>();
        _accessTokenGenerator = new Mock<IAccessTokenGenerator>();
        _accessTokenGenerator
            .Setup(generator => generator.Generate(UserId))
            .Returns(Result<AccessToken>.Success(AccessToken.Create("jwt", Now.AddMinutes(15)).Value));
        var options = Options.Create(new JwtOptions { RefreshTokenDays = 30, RefreshGraceSeconds = 10 });

        _useCase = new RefreshTokenCreate.Create(
            _queryBus.Object, _generator.Object, _refreshTokens.Object, _accessTokenGenerator.Object, _clock.Object, options);
    }

    [Test]
    public async Task UseCase_WithValidCredentials_ReturnsTheSessionAndStoresOnlyTheHashInANewFamily()
    {
        var result = await _useCase.ExecuteAsync(new RefreshTokenCreate.Command("ana@example.com", "right-password"));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(new SessionResponse("jwt", Now.AddMinutes(15), UserId, "Ana Perez", "raw-refresh")));
        _queryBus.Verify(bus => bus.Send(
            It.Is<UserPasswordVerify.Query>(query => query.Email == "ana@example.com" && query.Password == "right-password"),
            It.IsAny<CancellationToken>()), Times.Once);
        _refreshTokens.Verify(repository => repository.AddAsync(
            It.Is<RefreshToken>(token =>
                token.UserId == UserId
                && token.FamilyId != Guid.Empty
                && token.Id != token.FamilyId
                && token.TokenHash == "hash-refresh"
                && token.TokenHash != "raw-refresh"
                && token.ExpiresAt == Now.AddDays(30)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UseCase_WhenTheCredentialCheckFails_PropagatesTheFailureAndCreatesNothing()
    {
        _queryBus
            .Setup(bus => bus.Send(It.IsAny<UserPasswordVerify.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserResponse>.Failure(AuthenticationErrors.InvalidCredentials));

        var result = await _useCase.ExecuteAsync(new RefreshTokenCreate.Command("ana@example.com", "wrong"));

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(AuthenticationErrors.InvalidCredentials));
        _generator.Verify(generator => generator.Generate(), Times.Never);
        _refreshTokens.Verify(repository => repository.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
        _accessTokenGenerator.Verify(generator => generator.Generate(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task UseCase_WhenTheIssuerFails_PropagatesTheFailure()
    {
        _accessTokenGenerator.Setup(generator => generator.Generate(UserId)).Returns(Result<AccessToken>.Failure(AccessTokenErrors.ValueRequired));

        var result = await _useCase.ExecuteAsync(new RefreshTokenCreate.Command("ana@example.com", "right-password"));

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(AccessTokenErrors.ValueRequired));
    }

    [Test]
    public async Task Handler_DelegatesToTheUseCase()
    {
        var handler = new RefreshTokenCreate.Handler(_useCase);

        var result = await handler.Handle(new RefreshTokenCreate.Command("ana@example.com", "right-password"), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
    }
}
