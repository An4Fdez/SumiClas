namespace SumiClas.Tests.Sessions.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.Sessions.Application;
using SumiClas.Sessions.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Sessions.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class SessionRevokeShould : SessionsUnitTestCase
{
    private readonly Mock<IRefreshTokenGenerator> _refreshTokenGenerator = new();
    private readonly SessionRevoke.Revoke _useCase;

    public SessionRevokeShould()
    {
        this._useCase = new SessionRevoke.Revoke(
            this.Repository.Object,
            this._refreshTokenGenerator.Object,
            this.Clock.Object);
    }

    [Test]
    public async Task revoke_only_the_session_of_a_known_token()
    {
        this.ShouldHashTokens(this._refreshTokenGenerator);
        Guid userId = GuidMother.Random();
        string activeRaw = StringMother.Random("active");
        string otherRaw = StringMother.Random("other");
        Session session = SessionMother.With(userId, HashOf(activeRaw));
        Session other = SessionMother.With(userId, HashOf(otherRaw));
        this.ShouldSearchSessions(session, other);

        await this._useCase.ExecuteAsync(SessionRevokeCommandMother.WithToken(activeRaw));

        session.RevokedAt.Should().Be(Now);
        other.RevokedAt.Should().BeNull();
    }

    [Test]
    public async Task still_revoke_the_session_when_the_token_was_already_replaced()
    {
        this.ShouldHashTokens(this._refreshTokenGenerator);
        string oldRaw = StringMother.Random("old");
        string newRaw = StringMother.Random("new");
        Session session = SessionMother.With(GuidMother.Random(), HashOf(oldRaw));
        session.Rotate(HashOf(oldRaw), HashOf(newRaw), this.Clock.Object);
        this.ShouldSearchSessions(session);

        await this._useCase.ExecuteAsync(SessionRevokeCommandMother.WithToken(oldRaw));

        session.RevokedAt.Should().NotBeNull();
    }

    [TestCase(null)]
    [TestCase("  ")]
    [TestCase("unknown")]
    public async Task touch_nothing_when_the_token_is_blank_or_unknown(string? token)
    {
        this.ShouldHashTokens(this._refreshTokenGenerator);
        Session session = SessionMother.With(GuidMother.Random(), HashOf(StringMother.Random("raw")));
        this.ShouldSearchSessions(session);

        await this._useCase.ExecuteAsync(SessionRevokeCommandMother.WithToken(token));

        session.RevokedAt.Should().BeNull();
    }
}
