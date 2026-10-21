namespace SumiClas.Tests.Api;

using AwesomeAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SumiClas.Api.Configuration;
using SumiClas.Api.Http;
using SumiClas.SchoolYears.Domain;
using System.Net;
using System.Text.Json;

/// <summary>
/// The real exception handler middleware in front of a real endpoint, over a loopback socket: the handler must find
/// the declarations of the endpoint even though the middleware clears the endpoint of the request.
/// </summary>
[TestFixture]
public class DomainExceptionPipelineShould
{
    [Test]
    public async Task answer_a_declared_domain_exception_with_its_status_code_and_message()
    {
        await using WebApplication app = await StartAsync();
        using HttpClient client = ClientOf(app);

        HttpResponseMessage response = await client.GetAsync("/declared");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        JsonElement body = await BodyOf(response);
        body.GetProperty("code").GetString().Should().Be(SampleExceptions.NOT_FOUND_CODE);
        body.GetProperty("detail").GetString().Should().Be(SampleExceptions.NOT_FOUND_MESSAGE);
    }

    [Test]
    public async Task answer_a_500_without_code_when_the_endpoint_did_not_declare_the_domain_exception()
    {
        await using WebApplication app = await StartAsync();
        using HttpClient client = ClientOf(app);

        HttpResponseMessage response = await client.GetAsync("/undeclared");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        (await BodyOf(response)).TryGetProperty("code", out _).Should().BeFalse();
    }

    [TestCase("/not-yet-converted-archived", HttpStatusCode.BadRequest, "SchoolYear.Archived")]
    [TestCase("/not-yet-converted-missing", HttpStatusCode.NotFound, "SchoolYear.NotFound")]
    public async Task answer_with_the_status_of_its_code_a_domain_exception_from_an_endpoint_that_declares_none(
        string path,
        HttpStatusCode expected,
        string code)
    {
        await using WebApplication app = await StartAsync();
        using HttpClient client = ClientOf(app);

        HttpResponseMessage response = await client.GetAsync(path);

        response.StatusCode.Should().Be(expected);
        (await BodyOf(response)).GetProperty("code").GetString().Should().Be(code);
    }

    [Test]
    public async Task answer_a_500_for_an_exception_that_is_not_a_domain_exception()
    {
        await using WebApplication app = await StartAsync();
        using HttpClient client = ClientOf(app);

        HttpResponseMessage response = await client.GetAsync("/unexpected");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Test]
    public async Task log_as_an_error_an_exception_that_is_not_a_domain_exception()
    {
        CapturingLoggerProvider logs = new();
        await using WebApplication app = await StartAsync(logs);
        using HttpClient client = ClientOf(app);

        await client.GetAsync("/unexpected");

        logs.Entries.Should().Contain(entry =>
            entry.Level == LogLevel.Error && entry.Exception is InvalidOperationException);
    }

    private static async Task<WebApplication> StartAsync(CapturingLoggerProvider? logs = null)
    {
        WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        if (logs is not null)
        {
            builder.Logging.AddProvider(logs);
        }

        builder.Services.AddSumiClasExceptionHandling();
        WebApplication app = builder.Build();
        app.UseExceptionHandler();
        app.MapGet("/declared", IResult () => throw new SampleExceptions.NotFound())
            .Maps<SampleExceptions.NotFound>(StatusCodes.Status404NotFound);
        app.MapGet("/undeclared", IResult () => throw new SampleExceptions.Forbidden())
            .Maps<SampleExceptions.NotFound>(StatusCodes.Status404NotFound);
        app.MapGet("/unexpected", IResult () => throw new InvalidOperationException());
        app.MapGet("/not-yet-converted-archived", IResult () => throw new SchoolYearExceptions.Archived());
        app.MapGet("/not-yet-converted-missing", IResult () => throw new SchoolYearExceptions.NotFound());
        await app.StartAsync();
        return app;
    }

    private static HttpClient ClientOf(WebApplication app) => new() { BaseAddress = new Uri(app.Urls.First()) };

    private static async Task<JsonElement> BodyOf(HttpResponseMessage response)
    {
        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.Clone();
    }
}
