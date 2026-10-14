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

        Result result = await this._useCase.ExecuteAsync(SessionRevokeCommandMother.WithToken(activeRaw));

        result.IsSuccess.Should().BeTrue();
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

        Result result = await this._useCase.ExecuteAsync(SessionRevokeCommandMother.WithToken(oldRaw));

        result.IsSuccess.Should().BeTrue();
        session.RevokedAt.Should().NotBeNull();
    }

    [Test]
    public async Task keep_the_first_revocation_and_succeed_when_called_twice()
    {
        this.ShouldHashTokens(this._refreshTokenGenerator);
        string raw = StringMother.Random("raw");
        Session session = SessionMother.With(GuidMother.Random(), HashOf(raw));
        this.ShouldSearchSessions(session);
        await this._useCase.ExecuteAsync(SessionRevokeCommandMother.WithToken(raw));
        DateTimeOffset? firstRevocation = session.RevokedAt;
        this.AdvanceClock(TimeSpan.FromHours(1));

        Result result = await this._useCase.ExecuteAsync(SessionRevokeCommandMother.WithToken(raw));

        result.IsSuccess.Should().BeTrue();
        session.RevokedAt.Should().Be(firstRevocation);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("  ")]
    [TestCase("unknown")]
    public async Task succeed_and_touch_nothing_when_the_token_is_blank_or_unknown(string? token)
    {
        this.ShouldHashTokens(this._refreshTokenGenerator);
        Session session = SessionMother.With(GuidMother.Random(), HashOf(StringMother.Random("raw")));
        this.ShouldSearchSessions(session);

        Result result = await this._useCase.ExecuteAsync(SessionRevokeCommandMother.WithToken(token));

        result.IsSuccess.Should().BeTrue();
        session.RevokedAt.Should().BeNull();
    }
}
