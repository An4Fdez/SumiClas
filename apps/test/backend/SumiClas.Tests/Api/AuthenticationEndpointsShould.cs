namespace SumiClas.Tests.Api;

using AwesomeAssertions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SumiClas.Api.Configuration;
using SumiClas.Api.Endpoints;
using SumiClas.Api.Http;
using SumiClas.Sessions.Application;
using SumiClas.Sessions.Domain;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Sessions.Application;
using SumiClas.Tests.Shared;
using System.Net;
using System.Text.Json;

/// <summary>
/// The authentication endpoints over a real pipeline, with the use cases faked: the rule that a reused refresh token
/// revokes its session lives in the refresh endpoint, so it is proved here.
/// </summary>
[TestFixture]
public class AuthenticationEndpointsShould
{
    private const string LoginBody = """{"email":"a@b.com","password":"12345678"}""";

    private readonly Mock<ISender> _sender = new();
    private readonly RefreshCookieOptions _cookie = RefreshCookieOptionsMother.Default();

    [SetUp]
    public void SetUp()
    {
        this._sender.Reset();
    }

    [Test]
    public async Task revoke_the_session_clear_the_cookie_and_answer_401_when_the_refresh_token_was_reused()
    {
        string raw = StringMother.Random("raw");
        this.ShouldFailRotating(new SessionExceptions.RefreshTokenReused());
        await using WebApplication app = await StartAsync();

        HttpResponseMessage response = await PostRefreshAsync(app, raw);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await CodeOf(response)).Should().Be(AuthenticationErrors.RefreshTokenInvalid.Code);
        this.ShouldHaveClearedTheCookie(response);
        this.ShouldHaveRevokedWithoutBeingCancellable(raw);
    }

    [Test]
    public async Task clear_the_cookie_and_answer_401_without_revoking_when_the_refresh_token_is_invalid()
    {
        this.ShouldFailRotating(new AuthenticationExceptions.RefreshTokenInvalid());
        await using WebApplication app = await StartAsync();

        HttpResponseMessage response = await PostRefreshAsync(app, StringMother.Random("raw"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await CodeOf(response)).Should().Be(AuthenticationErrors.RefreshTokenInvalid.Code);
        this.ShouldHaveClearedTheCookie(response);
        this.ShouldNotHaveRevoked();
    }

    [Test]
    public async Task write_the_new_cookie_and_answer_the_session_without_the_refresh_token_when_the_rotation_succeeds()
    {
        string oldRaw = StringMother.Random("old");
        string newRaw = StringMother.Random("new");
        Guid userId = GuidMother.Random();
        string fullName = StringMother.Random("name");
        this._sender
            .Setup(sender => sender.Send(It.IsAny<SessionRotate.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SessionResponseMother.Create(
                StringMother.Random("jwt"),
                UnitTestCase.Now + AccessToken.Lifetime,
                userId,
                fullName,
                newRaw,
                UnitTestCase.Now + RefreshToken.Lifetime));
        await using WebApplication app = await StartAsync();

        HttpResponseMessage response = await PostRefreshAsync(app, oldRaw);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string cookies = string.Join(";", response.Headers.GetValues("Set-Cookie"));
        cookies.Should().Contain($"{this._cookie.Name}={newRaw}");
        string body = await response.Content.ReadAsStringAsync();
        body.Should().NotContain(newRaw);
        using JsonDocument document = JsonDocument.Parse(body);
        document.RootElement.GetProperty("userId").GetGuid().Should().Be(userId);
        document.RootElement.GetProperty("fullName").GetString().Should().Be(fullName);
        this.ShouldHaveRotated(oldRaw);
        this.ShouldNotHaveRevoked();
    }

    [Test]
    public async Task rotate_the_token_of_the_cookie_and_nothing_when_there_is_no_cookie()
    {
        this.ShouldFailRotating(new AuthenticationExceptions.RefreshTokenInvalid());
        await using WebApplication app = await StartAsync();

        HttpResponseMessage response = await PostRefreshAsync(app, null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        this.ShouldHaveRotated(null);
    }

    [Test]
    public async Task write_the_cookie_and_answer_the_session_when_the_credentials_are_valid()
    {
        string raw = StringMother.Random("raw");
        Guid userId = GuidMother.Random();
        this._sender
            .Setup(sender => sender.Send(It.IsAny<SessionCreate.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SessionResponseMother.Create(
                StringMother.Random("jwt"),
                UnitTestCase.Now + AccessToken.Lifetime,
                userId,
                StringMother.Random("name"),
                raw,
                UnitTestCase.Now + RefreshToken.Lifetime));
        await using WebApplication app = await StartAsync();

        HttpResponseMessage response = await PostAsync(app, "/auth/login", LoginBody);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string.Join(";", response.Headers.GetValues("Set-Cookie")).Should().Contain($"{this._cookie.Name}={raw}");
        (await response.Content.ReadAsStringAsync()).Should().NotContain(raw);
    }

    [Test]
    public async Task answer_401_with_the_invalid_credentials_code_and_no_cookie_when_the_credentials_are_wrong()
    {
        this._sender
            .Setup(sender => sender.Send(It.IsAny<SessionCreate.Command>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AuthenticationExceptions.InvalidCredentials());
        await using WebApplication app = await StartAsync();

        HttpResponseMessage response = await PostAsync(app, "/auth/login", LoginBody);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await CodeOf(response)).Should().Be(AuthenticationErrors.InvalidCredentials.Code);
        response.Headers.Contains("Set-Cookie").Should().BeFalse();
    }

    [Test]
    public async Task revoke_the_session_of_the_cookie_and_clear_it_when_logging_out()
    {
        string raw = StringMother.Random("raw");
        await using WebApplication app = await StartAsync();

        HttpResponseMessage response = await PostAsync(app, "/auth/logout", null, raw);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        this.ShouldHaveClearedTheCookie(response);
        this.ShouldHaveRevoked(raw);
    }

    private static async Task<HttpResponseMessage> PostAsync(
        WebApplication app,
        string path,
        string? json,
        string? cookie = null)
    {
        using HttpClient client = new() { BaseAddress = new Uri(app.Urls.First()) };
        using HttpRequestMessage request = new(HttpMethod.Post, path);
        if (json is not null)
        {
            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        }

        if (cookie is not null)
        {
            request.Headers.Add("Cookie", $"{RefreshCookieOptionsMother.Default().Name}={cookie}");
        }

        return await client.SendAsync(request);
    }

    private static async Task<HttpResponseMessage> PostRefreshAsync(WebApplication app, string? raw)
    {
        using HttpClient client = new() { BaseAddress = new Uri(app.Urls.First()) };
        using HttpRequestMessage request = new(HttpMethod.Post, "/auth/refresh");
        if (raw is not null)
        {
            request.Headers.Add("Cookie", $"{RefreshCookieOptionsMother.Default().Name}={raw}");
        }

        return await client.SendAsync(request);
    }

    private static async Task<string?> CodeOf(HttpResponseMessage response)
    {
        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("code").GetString();
    }

    private void ShouldHaveRevoked(string raw)
        => this._sender.Verify(
            sender => sender.Send(
                It.Is<SessionRevoke.Command>(command => command.RefreshToken == raw),
                It.IsAny<CancellationToken>()),
            Times.Once);

    private void ShouldHaveRevokedWithoutBeingCancellable(string raw)
        => this._sender.Verify(
            sender => sender.Send(
                It.Is<SessionRevoke.Command>(command => command.RefreshToken == raw),
                It.Is<CancellationToken>(token => !token.CanBeCanceled)),
            Times.Once);

    private void ShouldNotHaveRevoked()
        => this._sender.Verify(
            sender => sender.Send(It.IsAny<SessionRevoke.Command>(), It.IsAny<CancellationToken>()),
            Times.Never);

    private void ShouldHaveRotated(string? raw)
        => this._sender.Verify(
            sender => sender.Send(
                It.Is<SessionRotate.Command>(command => command.RefreshToken == raw),
                It.IsAny<CancellationToken>()),
            Times.Once);

    private void ShouldFailRotating(Exception exception)
        => this._sender
            .Setup(sender => sender.Send(It.IsAny<SessionRotate.Command>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

    private void ShouldHaveClearedTheCookie(HttpResponseMessage response)
    {
        string cookies = string.Join(";", response.Headers.GetValues("Set-Cookie")).ToLowerInvariant();
        cookies.Should().Contain($"{this._cookie.Name}=;");
        cookies.Should().Contain("max-age=0");
    }

    private async Task<WebApplication> StartAsync()
    {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddSumiClasExceptionHandling();
        builder.Services.AddSingleton(this._sender.Object);
        builder.Services.AddSingleton(Options.Create(this._cookie));
        builder.Services.AddSingleton(TimeProvider.System);
        WebApplication app = builder.Build();
        app.UseExceptionHandler();
        new AuthenticationEndpoints().Map(app);
        await app.StartAsync();
        return app;
    }
}
