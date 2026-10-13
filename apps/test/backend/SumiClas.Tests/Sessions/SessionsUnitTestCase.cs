namespace SumiClas.Tests.Sessions;

using Moq;
using SumiClas.Sessions.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;

public abstract class SessionsUnitTestCase : UnitTestCase
{
    protected const string HASH_PREFIX = "hash-of-";

    protected Mock<ISessionRepository> Repository { get; } = new();

    [SetUp]
    public void SessionsSetUp()
    {
        this.Repository.Reset();
        this.ShouldReturn();
    }

    protected static string HashOf(string raw) => HASH_PREFIX + raw;

    protected void ShouldHashTokens(Mock<IRefreshTokenGenerator> generator)
        => generator
            .Setup(refreshTokenGenerator => refreshTokenGenerator.Hash(It.IsAny<string>()))
            .Returns<string>(HashOf);

    protected void ShouldReturn(params Session[] sessions)
        => this.Repository
            .Setup(repository => repository.SearchAsync(
                It.IsAny<ISpecification<Session>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISpecification<Session> specification, CancellationToken _)
                => (IReadOnlyCollection<Session>)sessions.Where(specification.Criteria.Compile()).ToList());

    protected void ShouldHaveSaved(Guid userId, string tokenHash, DateTimeOffset expiresAt)
        => this.Repository.Verify(
            repository => repository.AddAsync(
                It.Is<Session>(session =>
                    session.UserId == userId
                    && session.Id != Guid.Empty
                    && session.RevokedAt == null
                    && session.RefreshTokens.Count == 1
                    && session.RefreshTokens.Single().Id != Guid.Empty
                    && session.RefreshTokens.Single().Id != session.Id
                    && session.RefreshTokens.Single().TokenHash == tokenHash
                    && session.RefreshTokens.Single().ExpiresAt == expiresAt),
                It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveSaved()
        => this.Repository.Verify(
            repository => repository.AddAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldNotHaveSearched()
        => this.Repository.Verify(
            repository => repository.SearchAsync(
                It.IsAny<ISpecification<Session>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
}
