namespace SumiClas.Tests.Api;

using AwesomeAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SumiClas.Api.Configuration;
using SumiClas.Api.Http;
using SumiClas.Shared.Domain;
using System.Net;
using System.Text.Json;

/// <summary>
/// The real exception handler middleware in front of a real endpoint, over a loopback socket: the handler must find
/// the declarations of the endpoint even though the middleware clears the endpoint of the request.
/// </summary>
[TestFixture]
public class ExceptionPipelineShould
{
    [Test]
    public async Task answer_a_declared_domain_exception_with_the_declared_status_code_and_message()
    {
        await using WebApplication app = await StartAsync();
        using HttpClient client = ClientOf(app);

        HttpResponseMessage response = await client.GetAsync("/declared");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        JsonElement body = await BodyOf(response);
        body.GetProperty("code").GetString().Should().Be(SampleExceptions.NOT_FOUND_CODE);
        body.GetProperty("detail").GetString().Should().Be(SampleExceptions.NOT_FOUND_MESSAGE);
    }

    [Test]
    public async Task answer_the_status_of_the_convention_when_the_endpoint_declared_another_domain_exception()
    {
        await using WebApplication app = await StartAsync();
        using HttpClient client = ClientOf(app);

        HttpResponseMessage response = await client.GetAsync("/undeclared");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await BodyOf(response)).GetProperty("code").GetString().Should().Be(SampleExceptions.FORBIDDEN_CODE);
    }

    [Test]
    public async Task answer_a_500_for_an_exception_that_is_not_a_domain_exception()
    {
        await using WebApplication app = await StartAsync();
        using HttpClient client = ClientOf(app);

        HttpResponseMessage response = await client.GetAsync("/unexpected");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        JsonElement body = await BodyOf(response);
        body.GetProperty("code").GetString().Should().Be(ApiErrorCodes.UnexpectedError);
        body.GetRawText().Should().NotContain(InfrastructureExceptionMother.INTERNAL_DETAIL);
    }

    [Test]
    public async Task log_as_an_error_an_exception_that_is_not_a_domain_exception()
    {
        Mock<ILogger> logger = LoggerMock.Enabled<ILogger>();
        await using WebApplication app = await StartAsync(LoggerMock.ProviderOf(logger.Object));
        using HttpClient client = ClientOf(app);

        await client.GetAsync("/unexpected");

        logger.ShouldHaveLoggedAnErrorCausedBy<ILogger, InvalidOperationException>();
    }

    private static async Task<WebApplication> StartAsync(ILoggerProvider? logs = null)
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
            .Maps<SampleExceptions.NotFound>(StatusCodes.Status409Conflict);
        app.MapGet("/undeclared", IResult () => throw new SampleExceptions.Forbidden())
            .Maps<SampleExceptions.NotFound>(StatusCodes.Status404NotFound);
        app.MapGet("/unexpected", IResult () => throw InfrastructureExceptionMother.Unexpected());
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
