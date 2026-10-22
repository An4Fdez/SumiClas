namespace SumiClas.Tests.Api;

using AwesomeAssertions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using SumiClas.Api.Http;
using SumiClas.Shared.Domain;
using System.Text.Json;

[TestFixture]
public class DomainExceptionHandlerShould : ApiUnitTestCase
{
    private readonly DomainExceptionHandler _handler = new(NullLogger<DomainExceptionHandler>.Instance);

    [Test]
    public async Task answer_with_the_status_the_endpoint_declared_and_the_code_and_message_of_the_exception()
    {
        DefaultHttpContext http = HttpContextMother.WithProblemDetails();
        http.SetEndpoint(EndpointMother.Declaring<SampleExceptions.NotFound>(StatusCodes.Status404NotFound));

        bool handled = await this._handler.TryHandleAsync(
            http,
            new SampleExceptions.NotFound(),
            CancellationToken.None);

        handled.Should().BeTrue();
        http.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        JsonElement body = await ReadBodyAsync(http);
        body.GetProperty("status").GetInt32().Should().Be(StatusCodes.Status404NotFound);
        body.GetProperty("code").GetString().Should().Be(SampleExceptions.NOT_FOUND_CODE);
        body.GetProperty("title").GetString().Should().Be(SampleExceptions.NOT_FOUND_CODE);
        body.GetProperty("detail").GetString().Should().Be(SampleExceptions.NOT_FOUND_MESSAGE);
    }

    [Test]
    public async Task read_the_endpoint_from_the_exception_handler_feature_when_the_request_no_longer_has_one()
    {
        DefaultHttpContext http = HttpContextMother.WithProblemDetails();
        SampleExceptions.Forbidden exception = new();
        http.Features.Set<IExceptionHandlerFeature>(new ExceptionHandlerFeature
        {
            Error = exception,
            Endpoint = EndpointMother.Declaring<SampleExceptions.Forbidden>(StatusCodes.Status403Forbidden),
        });

        bool handled = await this._handler.TryHandleAsync(http, exception, CancellationToken.None);

        handled.Should().BeTrue();
        http.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Test]
    public async Task pick_the_status_declared_for_the_exception_that_was_thrown()
    {
        DefaultHttpContext http = HttpContextMother.WithProblemDetails();
        http.SetEndpoint(EndpointMother.Declaring(
            new DomainExceptionStatus(typeof(SampleExceptions.NotFound), StatusCodes.Status404NotFound),
            new DomainExceptionStatus(typeof(SampleExceptions.Forbidden), StatusCodes.Status403Forbidden)));

        await this._handler.TryHandleAsync(http, new SampleExceptions.Forbidden(), CancellationToken.None);

        http.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Test]
    public async Task answer_a_plain_500_without_code_nor_message_when_the_endpoint_did_not_declare_the_exception()
    {
        DefaultHttpContext http = HttpContextMother.WithProblemDetails();
        http.SetEndpoint(EndpointMother.Declaring<SampleExceptions.NotFound>(StatusCodes.Status404NotFound));

        bool handled = await this._handler.TryHandleAsync(
            http,
            new SampleExceptions.Forbidden(),
            CancellationToken.None);

        handled.Should().BeTrue();
        http.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        JsonElement body = await ReadBodyAsync(http);
        body.TryGetProperty("code", out _).Should().BeFalse();
        body.TryGetProperty("detail", out _).Should().BeFalse();
    }

    [Test]
    public async Task answer_a_plain_500_when_the_request_has_no_endpoint()
    {
        DefaultHttpContext http = HttpContextMother.WithProblemDetails();

        bool handled = await this._handler.TryHandleAsync(
            http,
            new SampleExceptions.NotFound(),
            CancellationToken.None);

        handled.Should().BeTrue();
        http.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Test]
    public async Task answer_a_plain_500_when_the_endpoint_declares_no_domain_exceptions()
    {
        DefaultHttpContext http = HttpContextMother.WithProblemDetails();
        http.SetEndpoint(EndpointMother.Declaring());

        bool handled = await this._handler.TryHandleAsync(
            http,
            new SampleExceptions.NotFound(),
            CancellationToken.None);

        handled.Should().BeTrue();
        http.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        JsonElement body = await ReadBodyAsync(http);
        body.TryGetProperty("code", out _).Should().BeFalse();
    }

    [Test]
    public async Task leave_any_other_exception_alone()
    {
        DefaultHttpContext http = HttpContextMother.WithProblemDetails();
        http.SetEndpoint(EndpointMother.Declaring<SampleExceptions.NotFound>(StatusCodes.Status404NotFound));

        bool handled = await this._handler.TryHandleAsync(
            http,
            new InvalidOperationException(),
            CancellationToken.None);

        handled.Should().BeFalse();
        http.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        http.Response.Body.Length.Should().Be(0);
    }
}
