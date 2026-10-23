namespace SumiClas.Tests.Api;

using AwesomeAssertions;
using MediatR;
using Moq;
using SumiClas.Tests.Shared;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

/// <summary>
/// The brute-force brake of the authentication endpoints, over the real API with the use cases faked and the limit
/// lowered to two requests: login and registration share one counter, refresh and logout share another.
/// </summary>
[TestFixture]
public class RateLimitingShould
{
    private readonly Mock<ISender> _sender = new();

    [SetUp]
    public void SetUp()
    {
        this._sender.Reset();
    }

    [Test]
    public async Task let_the_login_attempts_through_up_to_the_limit_and_then_answer_429_with_retry_after()
    {
        await using EndpointsHost host = await EndpointsHost.StartAsync(
            this._sender.Object,
            RateLimitingSettingsMother.WithTheSmallLimits());
        using HttpClient client = host.CreateClient();

        for (int attempt = 1; attempt <= RateLimitingSettingsMother.PERMIT_LIMIT; attempt++)
        {
            using HttpResponseMessage allowed = await PostAsync(client, "/auth/login", RequestBodyMother.Login());
            allowed.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
        }

        using HttpResponseMessage refused = await PostAsync(client, "/auth/login", RequestBodyMother.Login());

        refused.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        refused.Headers.RetryAfter.Should().NotBeNull();
        refused.Headers.RetryAfter!.Delta.Should().BeGreaterThan(TimeSpan.Zero);
        (await CodeOf(refused)).Should().Be("Authentication.TooManyRequests");
    }

    [Test]
    public async Task share_the_login_counter_with_the_registration()
    {
        await using EndpointsHost host = await EndpointsHost.StartAsync(
            this._sender.Object,
            RateLimitingSettingsMother.WithTheSmallLimits());
        using HttpClient client = host.CreateClient();
        await ExhaustAsync(client, "/auth/login", RequestBodyMother.Login());

        using HttpResponseMessage response = await PostAsync(
            client,
            "/auth/register",
            RequestBodyMother.Registration());

        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Test]
    public async Task share_the_refresh_counter_with_the_logout()
    {
        await using EndpointsHost host = await EndpointsHost.StartAsync(
            this._sender.Object,
            RateLimitingSettingsMother.WithTheSmallLimits());
        using HttpClient client = host.CreateClient();
        await ExhaustAsync(client, "/auth/refresh", null);

        using HttpResponseMessage response = await PostAsync(client, "/auth/logout", null);

        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Test]
    public async Task count_refreshing_apart_from_the_login_attempts()
    {
        await using EndpointsHost host = await EndpointsHost.StartAsync(
            this._sender.Object,
            RateLimitingSettingsMother.WithTheSmallLimits());
        using HttpClient client = host.CreateClient();
        await ExhaustAsync(client, "/auth/login", RequestBodyMother.Login());

        using HttpResponseMessage response = await PostAsync(client, "/auth/refresh", null);

        response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
    }

    [Test]
    public async Task count_each_client_address_apart_so_that_one_cannot_lock_the_others_out()
    {
        await using EndpointsHost host = await EndpointsHost.StartAsync(
            this._sender.Object,
            RateLimitingSettingsMother.WithTheSmallLimits());
        using HttpClient attacker = host.CreateClient();
        await ExhaustAsync(attacker, "/auth/login", RequestBodyMother.Login());
        using HttpClient other = OtherAddressClient(host);

        using HttpResponseMessage response = await PostAsync(other, "/auth/login", RequestBodyMother.Login());

        response.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
    }

    [Test]
    public async Task not_limit_the_endpoints_outside_authentication()
    {
        await using EndpointsHost host = await EndpointsHost.StartAsync(
            this._sender.Object,
            RateLimitingSettingsMother.WithTheSmallLimits());
        using HttpClient client = host.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            host.AccessTokenOf(GuidMother.Random()));

        for (int request = 0; request <= RateLimitingSettingsMother.PERMIT_LIMIT * 3; request++)
        {
            using HttpResponseMessage response = await client.GetAsync("/users/me");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    private static HttpClient OtherAddressClient(EndpointsHost host)
    {
        IPAddress address = IPAddress.Parse("127.0.0.2");
        try
        {
            using Socket probe = new(SocketType.Stream, ProtocolType.Tcp);
            probe.Bind(new IPEndPoint(address, 0));
        }
        catch (SocketException)
        {
            Assert.Inconclusive("This machine cannot send from a second loopback address.");
        }

        return host.CreateClientFrom(address);
    }

    private static async Task ExhaustAsync(HttpClient client, string path, string? json)
    {
        for (int attempt = 1; attempt <= RateLimitingSettingsMother.PERMIT_LIMIT; attempt++)
        {
            using HttpResponseMessage allowed = await PostAsync(client, path, json);
            allowed.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
        }
    }

    private static async Task<HttpResponseMessage> PostAsync(HttpClient client, string path, string? json)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, path);
        if (json is not null)
        {
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return await client.SendAsync(request);
    }

    private static async Task<string?> CodeOf(HttpResponseMessage response)
    {
        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("code").GetString();
    }
}
