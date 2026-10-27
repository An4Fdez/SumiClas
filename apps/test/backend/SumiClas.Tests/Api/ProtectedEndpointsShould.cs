namespace SumiClas.Tests.Api;

using AwesomeAssertions;
using MediatR;
using Moq;
using SumiClas.Sessions.Application;
using SumiClas.Tests.Shared;
using SumiClas.Users.Application;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static SumiClas.Api.Endpoints.EndpointsExtensions;

/// <summary>
/// The perimeter of the real API: every endpoint outside /auth asks for a session. The endpoints are the ones the API
/// really maps, not a list written by hand, so a new endpoint left open breaks this test.
/// </summary>
[TestFixture]
public class ProtectedEndpointsShould
{
    private const string UNAUTHENTICATED_CODE = "Authentication.Unauthenticated";
    private const int MINIMUM_PROTECTED_ENDPOINTS = 35;

    private readonly Mock<ISender> _sender = new();

    [SetUp]
    public void SetUp()
    {
        this._sender.Reset();
    }

    [Test]
    public async Task answer_401_to_every_endpoint_outside_authentication_when_there_is_no_token()
    {
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = host.CreateClient();
        List<(string Method, string Pattern)> endpoints = host.Endpoints()
            .Where(endpoint => !IsAuthentication(endpoint.Pattern))
            .ToList();

        List<string> open = [];
        foreach ((string method, string pattern) in endpoints)
        {
            using HttpRequestMessage request = new(new HttpMethod(method), WithValues(pattern));
            using HttpResponseMessage response = await client.SendAsync(request);
            if (response.StatusCode != HttpStatusCode.Unauthorized || await CodeOf(response) != UNAUTHENTICATED_CODE)
            {
                open.Add($"{method} {pattern} answered {(int)response.StatusCode}");
            }
        }

        endpoints.Count.Should().BeGreaterThanOrEqualTo(MINIMUM_PROTECTED_ENDPOINTS);
        open.Should().BeEmpty();
        this._sender.VerifyNoOtherCalls();
    }

    [Test]
    public async Task let_a_request_with_a_valid_token_reach_the_use_case()
    {
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = host.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            host.AccessTokenOf(GuidMother.Random()));

        using HttpResponseMessage response = await client.GetAsync($"{ApiPrefix}/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        this.ShouldHaveAskedForTheProfileOfTheCurrentUser();
    }

    [TestCase($"{ApiPrefix}/auth/register", typeof(UserRegister.Command), false)]
    [TestCase($"{ApiPrefix}/auth/login", typeof(SessionCreate.Command), false)]
    [TestCase($"{ApiPrefix}/auth/refresh", typeof(SessionRotate.Command), true)]
    [TestCase($"{ApiPrefix}/auth/logout", typeof(SessionRevoke.Command), true)]
    public async Task let_the_authentication_endpoints_reach_their_use_case_without_a_session(
        string path,
        Type command,
        bool withRefreshCookie)
    {
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = host.CreateClient();
        using HttpRequestMessage request = new(HttpMethod.Post, path)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json"),
        };
        if (withRefreshCookie)
        {
            request.Headers.Add("Cookie", $"{RefreshCookieOptionsMother.Default().Name}={StringMother.Random("raw")}");
        }

        using HttpResponseMessage response = await client.SendAsync(request);

        (await CodeOf(response)).Should().NotBe(UNAUTHENTICATED_CODE);
        response.Headers.WwwAuthenticate.Should().BeEmpty();
        this.ShouldHaveReceived(command);
    }

    private static bool IsAuthentication(string pattern) =>
        pattern.StartsWith($"{ApiPrefix}/auth/", StringComparison.Ordinal);

    private static string WithValues(string pattern)
        => Regex.Replace(pattern, "{[^}]+}", _ => GuidMother.Random().ToString());

    private static async Task<string?> CodeOf(HttpResponseMessage response)
    {
        string body = await response.Content.ReadAsStringAsync();
        if (body.Length == 0)
        {
            return null;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(body);
            return document.RootElement.TryGetProperty("code", out JsonElement code) ? code.GetString() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private void ShouldHaveReceived(Type command)
        => this._sender.Invocations
            .Select(invocation => invocation.Arguments.FirstOrDefault()?.GetType())
            .Should().Contain(command);

    private void ShouldHaveAskedForTheProfileOfTheCurrentUser()
        => this._sender.Verify(
            sender => sender.Send(It.IsAny<UserProfileFinderCurrent.Query>(), It.IsAny<CancellationToken>()),
            Times.Once);
}
