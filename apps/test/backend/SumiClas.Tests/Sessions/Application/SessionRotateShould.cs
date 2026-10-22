namespace SumiClas.Tests.Sessions.Application;

using AwesomeAssertions;
using Moq;
using SumiClas.Sessions.Application;
using SumiClas.Sessions.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Sessions.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Users.Domain;
using SumiClas.Users.Application;

[TestFixture]
public class SessionRotateShould : SessionsUnitTestCase
{
    private readonly Mock<IRefreshTokenGenerator> _refreshTokenGenerator = new();
    private readonly Mock<IAccessTokenGenerator> _accessTokenGenerator = new();
    private readonly SessionRotate.Rotate _useCase;

    public SessionRotateShould()
    {
        this._useCase = new SessionRotate.Rotate(
            this.Repository.Object,
            this._refreshTokenGenerator.Object,
            this._accessTokenGenerator.Object,
            this.QueryBus.Object,
            this.Clock.Object);
    }

    [Test]
    public async Task throw_the_access_token_failure_when_the_access_token_issuer_fails()
    {
        this.ShouldHashTokens(this._refreshTokenGenerator);
        Guid userId = GuidMother.Random();
        string oldRaw = StringMother.Random("old");
        this.ShouldFindUser(userId, StringMother.Random("name"));
        this.ShouldGenerateRefreshToken(StringMother.Random("new"));
        this.ShouldSearchSessions(SessionMother.With(userId, HashOf(oldRaw)));
        this.ShouldFailGeneratingAccessToken(userId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            SessionRotateCommandMother.WithToken(oldRaw));

        await act.Should().ThrowAsync<AccessTokenExceptions.ValueRequired>();
    }

    [Test]
    public async Task rotate_the_token_and_return_the_new_session_when_the_token_is_active()
    {
        this.ShouldHashTokens(this._refreshTokenGenerator);
        Guid userId = GuidMother.Random();
        string fullName = StringMother.Random("name");
        string jwt = StringMother.Random("jwt");
        string oldRaw = StringMother.Random("old");
        string newRaw = StringMother.Random("new");
        this.ShouldFindUser(userId, fullName);
        this.ShouldGenerateRefreshToken(newRaw);
        this.ShouldGenerateAccessToken(userId, jwt);
        Session session = SessionMother.With(userId, HashOf(oldRaw));
        this.ShouldSearchSessions(session);

        SessionResponse result = await this._useCase.ExecuteAsync(
            SessionRotateCommandMother.WithToken(oldRaw));

        result.Should().Be(SessionResponseMother.Create(
            jwt,
            Now + AccessToken.Lifetime,
            userId,
            fullName,
            newRaw,
            Now + RefreshToken.Lifetime));
        RefreshToken old = TokenOf(session, oldRaw);
        RefreshToken successor = TokenOf(session, newRaw);
        session.RefreshTokens.Should().HaveCount(2);
        successor.TokenHash.Should().Be(HashOf(newRaw));
        successor.ExpiresAt.Should().Be(Now + RefreshToken.Lifetime);
        successor.Id.Should().NotBe(Guid.Empty);
        successor.Id.Should().NotBe(session.Id);
        successor.Id.Should().NotBe(old.Id);
        old.ReplacedById.Should().Be(successor.Id);
        old.ReplacedAt.Should().Be(Now);
    }

    [TestCase(null)]
    [TestCase("   ")]
    public async Task throw_invalid_and_search_nothing_when_the_token_is_blank(string? raw)
    {
        Func<Task> act = () => this._useCase.ExecuteAsync(
            SessionRotateCommandMother.WithToken(raw));

        await act.Should().ThrowAsync<AuthenticationExceptions.RefreshTokenInvalid>();
        this.ShouldNotHaveSearched();
    }

    [Test]
    public async Task throw_invalid_and_change_nothing_when_the_token_is_unknown()
    {
        this.ShouldHashTokens(this._refreshTokenGenerator);
        string otherRaw = StringMother.Random("other");
        string unknownRaw = StringMother.Random("unknown");
        Session session = SessionMother.With(GuidMother.Random(), HashOf(otherRaw));
        this.ShouldSearchSessions(session);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            SessionRotateCommandMother.WithToken(unknownRaw));


        await act.Should().ThrowAsync<AuthenticationExceptions.RefreshTokenInvalid>();
        session.RefreshTokens.Should().HaveCount(1);
    }

    [Test]
    public async Task throw_invalid_and_change_nothing_when_the_token_is_expired()
    {
        this.ShouldHashTokens(this._refreshTokenGenerator);
        Guid userId = GuidMother.Random();
        string oldRaw = StringMother.Random("old");
        this.ShouldFindUser(userId, StringMother.Random("name"));
        this.ShouldGenerateRefreshToken(StringMother.Random("new"));
        Session session = SessionMother.With(userId, HashOf(oldRaw));
        this.ShouldSearchSessions(session);
        this.AdvanceClock(RefreshToken.Lifetime + TimeSpan.FromSeconds(1));

        Func<Task> act = () => this._useCase.ExecuteAsync(
            SessionRotateCommandMother.WithToken(oldRaw));


        await act.Should().ThrowAsync<AuthenticationExceptions.RefreshTokenInvalid>();
        TokenOf(session, oldRaw).ReplacedAt.Should().BeNull();
        session.RefreshTokens.Should().HaveCount(1);
    }

    [Test]
    public async Task throw_invalid_and_change_nothing_when_the_session_is_revoked()
    {
        this.ShouldHashTokens(this._refreshTokenGenerator);
        Guid userId = GuidMother.Random();
        string oldRaw = StringMother.Random("old");
        this.ShouldFindUser(userId, StringMother.Random("name"));
        this.ShouldGenerateRefreshToken(StringMother.Random("new"));
        Session session = SessionMother.With(userId, HashOf(oldRaw));
        session.Revoke(this.Clock.Object);
        this.ShouldSearchSessions(session);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            SessionRotateCommandMother.WithToken(oldRaw));


        await act.Should().ThrowAsync<AuthenticationExceptions.RefreshTokenInvalid>();
        TokenOf(session, oldRaw).ReplacedAt.Should().BeNull();
        session.RefreshTokens.Should().HaveCount(1);
    }

    [Test]
    public async Task revoke_the_session_and_throw_reuse_when_a_replaced_token_comes_after_the_grace()
    {
        this.ShouldHashTokens(this._refreshTokenGenerator);
        Guid userId = GuidMother.Random();
        string oldRaw = StringMother.Random("old");
        string unrelatedRaw = StringMother.Random("unrelated");
        this.ShouldFindUser(userId, StringMother.Random("name"));
        this.ShouldGenerateRefreshToken(StringMother.Random("new"));
        this.ShouldGenerateAccessToken(userId, StringMother.Random("jwt"));
        Session session = SessionMother.With(userId, HashOf(oldRaw));
        Session unrelated = SessionMother.With(userId, HashOf(unrelatedRaw));
        this.ShouldSearchSessions(session, unrelated);
        await this._useCase.ExecuteAsync(SessionRotateCommandMother.WithToken(oldRaw));
        this.AdvanceClock(Session.RefreshGrace + TimeSpan.FromSeconds(1));

        Func<Task> act = () => this._useCase.ExecuteAsync(
            SessionRotateCommandMother.WithToken(oldRaw));


        await act.Should().ThrowAsync<SessionExceptions.RefreshTokenReused>();
        session.RevokedAt.Should().NotBeNull();
        unrelated.RevokedAt.Should().BeNull();
        session.RefreshTokens.Should().HaveCount(2);
    }

    [Test]
    public async Task throw_invalid_and_not_revoke_the_session_when_a_replaced_token_comes_within_the_grace()
    {
        this.ShouldHashTokens(this._refreshTokenGenerator);
        Guid userId = GuidMother.Random();
        string oldRaw = StringMother.Random("old");
        this.ShouldFindUser(userId, StringMother.Random("name"));
        this.ShouldGenerateRefreshToken(StringMother.Random("new"));
        this.ShouldGenerateAccessToken(userId, StringMother.Random("jwt"));
        Session session = SessionMother.With(userId, HashOf(oldRaw));
        this.ShouldSearchSessions(session);
        await this._useCase.ExecuteAsync(SessionRotateCommandMother.WithToken(oldRaw));
        this.AdvanceClock(Session.RefreshGrace / 2);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            SessionRotateCommandMother.WithToken(oldRaw));


        await act.Should().ThrowAsync<AuthenticationExceptions.RefreshTokenInvalid>();
        session.RevokedAt.Should().BeNull();
        session.RefreshTokens.Should().HaveCount(2);
    }

    [Test]
    public async Task throw_invalid_and_leave_the_session_untouched_when_the_user_no_longer_exists()
    {
        this.ShouldHashTokens(this._refreshTokenGenerator);
        Guid userId = GuidMother.Random();
        string oldRaw = StringMother.Random("old");
        Session session = SessionMother.With(userId, HashOf(oldRaw));
        this.ShouldSearchSessions(session);
        this.ShouldNotFindUser(userId);

        Func<Task> act = () => this._useCase.ExecuteAsync(
            SessionRotateCommandMother.WithToken(oldRaw));


        await act.Should().ThrowAsync<AuthenticationExceptions.RefreshTokenInvalid>();
        TokenOf(session, oldRaw).ReplacedAt.Should().BeNull();
        session.RefreshTokens.Should().HaveCount(1);
    }

    private static RefreshToken TokenOf(Session session, string raw)
        => session.RefreshTokens.Single(token => token.TokenHash == HashOf(raw));

    private void ShouldFindUser(Guid userId, string fullName)
        => this.QueryBus
            .Setup(bus => bus.Send(
                It.Is<UserFinderByIdOrNull.Query>(query => query.UserId == userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(UserResponseMother.With(userId, fullName));

    private void ShouldGenerateRefreshToken(string raw)
        => this._refreshTokenGenerator.Setup(generator => generator.Generate()).Returns(raw);

    private void ShouldGenerateAccessToken(Guid userId, string jwt)
        => this._accessTokenGenerator
            .Setup(generator => generator.Generate(userId))
            .Returns(AccessTokenMother.With(jwt));

    private void ShouldFailGeneratingAccessToken(Guid userId)
        => this._accessTokenGenerator
            .Setup(generator => generator.Generate(userId))
            .Throws(new AccessTokenExceptions.ValueRequired());
}
