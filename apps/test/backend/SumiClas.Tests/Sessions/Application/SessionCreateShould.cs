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
        this.ShouldIssueSession(userId, fullName, jwt, rawRefresh);
        SessionCreate.Command command = SessionCreateCommandMother.WithCredentials(email, password);

        Result<SessionResponse> result = await this._useCase.ExecuteAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(SessionResponseMother.Create(
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
    public async Task propagate_the_failure_and_create_nothing_when_the_credential_check_fails()
    {
        string email = StringMother.Random("email") + "@example.com";
        this.ShouldIssueSession(
            GuidMother.Random(),
            StringMother.Random("name"),
            StringMother.Random("jwt"),
            StringMother.Random("raw"));
        this.ShouldRejectCredentials();
        SessionCreate.Command command =
            SessionCreateCommandMother.WithCredentials(email, StringMother.Random("password"));

        Result<SessionResponse> result = await this._useCase.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthenticationErrors.InvalidCredentials);
        this._refreshTokenGenerator.Verify(generator => generator.Generate(), Times.Never);
        this.ShouldNotHaveSaved();
        this._accessTokenGenerator.Verify(generator => generator.Generate(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task propagate_the_failure_when_the_access_token_issuer_fails()
    {
        this.ShouldHashTokens(this._refreshTokenGenerator);
        Guid userId = GuidMother.Random();
        string email = StringMother.Random("email") + "@example.com";
        string password = StringMother.Random("password");
        this.ShouldIssueSession(
            userId,
            StringMother.Random("name"),
            StringMother.Random("jwt"),
            StringMother.Random("raw"));
        this._accessTokenGenerator
            .Setup(generator => generator.Generate(userId))
            .Returns(Result<AccessToken>.Failure(AccessTokenErrors.ValueRequired));
        SessionCreate.Command command = SessionCreateCommandMother.WithCredentials(email, password);

        Result<SessionResponse> result = await this._useCase.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AccessTokenErrors.ValueRequired);
    }

    [Test]
    public async Task return_the_use_case_result_when_the_handler_handles_the_command()
    {
        this.ShouldHashTokens(this._refreshTokenGenerator);
        string email = StringMother.Random("email") + "@example.com";
        string password = StringMother.Random("password");
        this.ShouldIssueSession(
            GuidMother.Random(),
            StringMother.Random("name"),
            StringMother.Random("jwt"),
            StringMother.Random("raw"));
        SessionCreate.Handler handler = new(this._useCase);
        SessionCreate.Command command = SessionCreateCommandMother.WithCredentials(email, password);

        Result<SessionResponse> result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    private void ShouldRejectCredentials()
        => this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<UserPasswordVerify.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserResponse>.Failure(AuthenticationErrors.InvalidCredentials));

    private void ShouldIssueSession(Guid userId, string fullName, string jwt, string rawRefresh)
    {
        this.QueryBus
            .Setup(bus => bus.Send(It.IsAny<UserPasswordVerify.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserResponse>.Success(UserResponseMother.With(userId, fullName)));
        this._refreshTokenGenerator.Setup(generator => generator.Generate()).Returns(rawRefresh);
        this._accessTokenGenerator
            .Setup(generator => generator.Generate(userId))
            .Returns(Result<AccessToken>.Success(AccessTokenMother.With(jwt)));
    }
}
