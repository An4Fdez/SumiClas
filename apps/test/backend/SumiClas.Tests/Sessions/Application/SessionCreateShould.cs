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
public class SessionCreateShould : SessionsUnitTestCase
{
    private readonly Mock<IRefreshTokenGenerator> _refreshTokenGenerator = new();
    private readonly Mock<IAccessTokenGenerator> _accessTokenGenerator = new();
    private readonly SessionCreate.Create _useCase;

    public SessionCreateShould()
    {
        this._useCase = new SessionCreate.Create(
            this.QueryBus.Object,
            this._refreshTokenGenerator.Object,
            this.Repository.Object,
            this._accessTokenGenerator.Object,
            this.Clock.Object);
    }

    [SetUp]
    public void SetUp()
    {
        this._refreshTokenGenerator.Reset();
        this._accessTokenGenerator.Reset();
    }

    [Test]
    public async Task return_the_session_and_store_only_the_hash_in_a_new_session_when_credentials_are_valid()
    {
        this.ShouldHashTokens(this._refreshTokenGenerator);
        Guid userId = GuidMother.Random();
        string fullName = StringMother.Random("name");
        string email = StringMother.Random("email") + "@example.com";
        string password = StringMother.Random("password");
        string jwt = StringMother.Random("jwt");
        string rawRefresh = StringMother.Random("raw");
        this.ShouldAcceptCredentials(userId, fullName);
        this.ShouldGenerateRefreshToken(rawRefresh);
        this.ShouldGenerateAccessToken(userId, jwt);
        SessionCreate.Command command = SessionCreateCommandMother.WithCredentials(email, password);

        SessionResponse result = await this._useCase.ExecuteAsync(command);

        result.Should().Be(SessionResponseMother.Create(
            jwt,
            Now + AccessToken.Lifetime,
            userId,
            fullName,
            rawRefresh,
            Now + RefreshToken.Lifetime));
        this.QueryBus.Verify(
            bus => bus.Send(
                It.Is<UserPasswordVerify.Query>(
                    query => query.Email == email && query.Password == password),
                It.IsAny<CancellationToken>()),
            Times.Once);
        this.ShouldHaveSaved(userId, HashOf(rawRefresh), Now + RefreshToken.Lifetime);
    }

    [Test]
    public async Task throw_and_create_nothing_when_the_credential_check_fails()
    {
        string email = StringMother.Random("email") + "@example.com";
        this.ShouldRejectCredentials();
        SessionCreate.Command command =
            SessionCreateCommandMother.WithCredentials(email, StringMother.Random("password"));

        Func<Task> act = () => this._useCase.ExecuteAsync(command);

        await act.Should().ThrowAsync<AuthenticationExceptions.InvalidCredentials>();
        this._refreshTokenGenerator.Verify(generator => generator.Generate(), Times.Never);
        this.ShouldNotHaveSaved();
        this._accessTokenGenerator.Verify(generator => generator.Generate(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task throw_the_access_token_failure_when_the_access_token_issuer_fails()
    {
        this.ShouldHashTokens(this._refreshTokenGenerator);
        Guid userId = GuidMother.Random();
        string email = StringMother.Random("email") + "@example.com";
        string password = StringMother.Random("password");
        this.ShouldAcceptCredentials(userId, StringMother.Random("name"));
        this.ShouldGenerateRefreshToken(StringMother.Random("raw"));
        this.ShouldFailGeneratingAccessToken(userId);
        SessionCreate.Command command = SessionCreateCommandMother.WithCredentials(email, password);

        Func<Task> act = () => this._useCase.ExecuteAsync(command);

        await act.Should().ThrowAsync<AccessTokenExceptions.ValueRequired>();
    }

    private void ShouldRejectCredentials()
        => this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<UserPasswordVerify.Query>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AuthenticationExceptions.InvalidCredentials());

    private void ShouldAcceptCredentials(Guid userId, string fullName)
        => this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<UserPasswordVerify.Query>(), It.IsAny<CancellationToken>()))
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
