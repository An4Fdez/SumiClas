namespace SumiClas.Tests.Api;

using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SumiClas.Api.Configuration;
using SumiClas.Api.Endpoints;
using SumiClas.Api.Http;
using SumiClas.Sessions.Infrastructure;
using SumiClas.Tests.Sessions.Infrastructure;
using System.Net;
using System.Net.Sockets;

/// <summary>
/// The real API over a loopback socket, composed as Program.cs does (security, exception handling, rate limiting,
/// validation and every endpoint) but with the use cases faked, so it needs no database.
/// </summary>
public sealed class EndpointsHost : IAsyncDisposable
{
    private readonly WebApplication _app;
    private readonly JwtOptions _jwt;

    private EndpointsHost(WebApplication app, JwtOptions jwt)
    {
        this._app = app;
        this._jwt = jwt;
    }

    public static async Task<EndpointsHost> StartAsync(
        ISender sender,
        IReadOnlyDictionary<string, string?>? settings = null)
    {
        JwtOptions jwt = JwtOptionsMother.Random();
        Dictionary<string, string?> all = JwtSettingsMother.From(jwt);
        foreach (KeyValuePair<string, string?> setting in settings ?? new Dictionary<string, string?>())
        {
            all[setting.Key] = setting.Value;
        }

        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services
            .AddSumiClasJwtOptions(ConfigurationMother.Create(all))
            .AddSumiClasApiSecurity(ConfigurationMother.Create(all))
            .AddSumiClasRateLimiting(ConfigurationMother.Create(all))
            .AddSumiClasExceptionHandling();
        builder.Services.AddSumiClasValidation();
        builder.Services.AddSingleton(sender);
        builder.Services.AddSingleton(Options.Create(RefreshCookieOptionsMother.Default()));
        builder.Services.AddSingleton(TimeProvider.System);
        WebApplication app = builder.Build();
        app.UseExceptionHandler();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapEndpoints();
        await app.StartAsync();
        return new EndpointsHost(app, jwt);
    }

    /// <summary>Every endpoint the real API maps, as its HTTP method and its route pattern.</summary>
    public IReadOnlyList<(string Method, string Pattern)> Endpoints()
        => ((IEndpointRouteBuilder)this._app).DataSources
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .SelectMany(endpoint => (endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods ?? [])
                .Select(method => (method, endpoint.RoutePattern.RawText ?? string.Empty)))
            .ToList();

    public HttpClient CreateClient() => new() { BaseAddress = new Uri(this._app.Urls.First()) };

    /// <summary>A client that reaches the API from another loopback address, to tell clients apart by IP.</summary>
    public HttpClient CreateClientFrom(IPAddress address)
    {
        SocketsHttpHandler handler = new()
        {
            ConnectCallback = async (context, ct) =>
            {
                Socket socket = new(SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    socket.Bind(new IPEndPoint(address, 0));
                    await socket.ConnectAsync(context.DnsEndPoint, ct);
                    return new NetworkStream(socket, ownsSocket: true);
                }
                catch
                {
                    socket.Dispose();
                    throw;
                }
            },
        };
        return new HttpClient(handler) { BaseAddress = new Uri(this._app.Urls.First()) };
    }

    public string AccessTokenOf(Guid userId)
        => JwtTokenMother.Create(
            userId,
            this._jwt.Issuer,
            this._jwt.Audience,
            this._jwt.SigningKey,
            "HS256",
            "at+jwt",
            DateTimeOffset.UtcNow);

    public async ValueTask DisposeAsync() => await this._app.DisposeAsync();
}
