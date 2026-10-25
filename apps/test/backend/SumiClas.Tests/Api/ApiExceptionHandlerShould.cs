namespace SumiClas.Tests.Api;

using AwesomeAssertions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SumiClas.Api.Http;
using System.Text.Json;

[TestFixture]
public class ApiExceptionHandlerShould : ApiUnitTestCase
{
    private readonly ApiExceptionHandler _handler = new(NullLogger<ApiExceptionHandler>.Instance);

    [Test]
    public async Task answer_with_the_status_the_endpoint_declared_over_the_convention_and_the_code_and_message()
    {
        DefaultHttpContext http = HttpContextMother.WithProblemDetails();
        http.SetEndpoint(EndpointMother.Declaring<SampleExceptions.NotFound>(StatusCodes.Status409Conflict));

        bool handled = await this._handler.TryHandleAsync(
            http,
            new SampleExceptions.NotFound(),
            CancellationToken.None);

        handled.Should().BeTrue();
        http.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        JsonElement body = await ReadBodyAsync(http);
        body.GetProperty("status").GetInt32().Should().Be(StatusCodes.Status409Conflict);
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
            Endpoint = EndpointMother.Declaring<SampleExceptions.Forbidden>(StatusCodes.Status400BadRequest),
        });

        bool handled = await this._handler.TryHandleAsync(http, exception, CancellationToken.None);

        handled.Should().BeTrue();
        http.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Test]
    public async Task pick_the_status_declared_for_the_exception_that_was_thrown()
    {
        DefaultHttpContext http = HttpContextMother.WithProblemDetails();
        http.SetEndpoint(EndpointMother.Declaring(
            new DomainExceptionStatus(typeof(SampleExceptions.NotFound), StatusCodes.Status409Conflict),
            new DomainExceptionStatus(typeof(SampleExceptions.Forbidden), StatusCodes.Status400BadRequest)));

        await this._handler.TryHandleAsync(http, new SampleExceptions.Forbidden(), CancellationToken.None);

        http.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Test]
    public async Task answer_the_status_of_the_convention_with_its_code_when_another_exception_was_declared()
    {
        DefaultHttpContext http = HttpContextMother.WithProblemDetails();
        http.SetEndpoint(EndpointMother.Declaring<SampleExceptions.NotFound>(StatusCodes.Status409Conflict));

        bool handled = await this._handler.TryHandleAsync(
            http,
            new SampleExceptions.Forbidden(),
            CancellationToken.None);

        handled.Should().BeTrue();
        http.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        JsonElement body = await ReadBodyAsync(http);
        body.GetProperty("code").GetString().Should().Be(SampleExceptions.FORBIDDEN_CODE);
        body.GetProperty("detail").GetString().Should().Be(SampleExceptions.FORBIDDEN_MESSAGE);
    }

    [Test]
    public async Task answer_the_status_of_the_convention_when_the_request_has_no_endpoint()
    {
        DefaultHttpContext http = HttpContextMother.WithProblemDetails();

        bool handled = await this._handler.TryHandleAsync(
            http,
            new SampleExceptions.NotFound(),
            CancellationToken.None);

        handled.Should().BeTrue();
        http.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Test]
    public async Task answer_the_status_of_the_convention_when_the_endpoint_declares_no_domain_exceptions()
    {
        DefaultHttpContext http = HttpContextMother.WithProblemDetails();
        http.SetEndpoint(EndpointMother.Declaring());

        bool handled = await this._handler.TryHandleAsync(
            http,
            new SampleExceptions.NotFound(),
            CancellationToken.None);

        handled.Should().BeTrue();
        http.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        JsonElement body = await ReadBodyAsync(http);
        body.GetProperty("code").GetString().Should().Be(SampleExceptions.NOT_FOUND_CODE);
        body.GetProperty("detail").GetString().Should().Be(SampleExceptions.NOT_FOUND_MESSAGE);
    }

    [Test]
    public async Task answer_500_with_the_unexpected_code_and_no_internal_detail()
    {
        DefaultHttpContext http = HttpContextMother.WithProblemDetails();

        bool handled = await this._handler.TryHandleAsync(
            http,
            InfrastructureExceptionMother.Unexpected(),
            CancellationToken.None);

        handled.Should().BeTrue();
        http.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        JsonElement body = await ReadBodyAsync(http);
        body.GetProperty("code").GetString().Should().Be(ApiErrorCodes.UnexpectedError);
        body.GetProperty("traceId").GetString().Should().NotBeNullOrWhiteSpace();
        body.GetRawText().Should().NotContain(InfrastructureExceptionMother.INTERNAL_DETAIL);
    }

    [Test]
    public async Task log_the_unexpected_error_with_the_trace_id_the_response_carries()
    {
        CapturingLoggerProvider logs = new();
        DefaultHttpContext http = HttpContextMother.WithProblemDetails();
        Exception exception = InfrastructureExceptionMother.Unexpected();

        await HandlerLoggingTo(logs).TryHandleAsync(http, exception, CancellationToken.None);

        string traceId = (await ReadBodyAsync(http)).GetProperty("traceId").GetString()!;
        logs.Entries.Should().ContainSingle(entry => entry.Level == LogLevel.Error)
            .Which.Should().Match<CapturingLoggerProvider.LogEntry>(
                entry => ReferenceEquals(entry.Exception, exception) && entry.Message.Contains(traceId));
    }

    [Test]
    public async Task answer_503_with_retry_after_when_the_database_is_not_available()
    {
        DefaultHttpContext http = HttpContextMother.WithProblemDetails();

        bool handled = await this._handler.TryHandleAsync(
            http,
            InfrastructureExceptionMother.TransientDatabase(),
            CancellationToken.None);

        handled.Should().BeTrue();
        http.Response.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
        http.Response.Headers.RetryAfter.ToString().Should().Be(ApiExceptionHandler.RetryAfterSeconds.ToString());
        (await ReadBodyAsync(http)).GetProperty("code").GetString().Should().Be(ApiErrorCodes.ServiceUnavailable);
    }

    [Test]
    public async Task answer_503_when_a_database_command_timed_out()
    {
        DefaultHttpContext http = HttpContextMother.WithProblemDetails();

        await this._handler.TryHandleAsync(
            http,
            InfrastructureExceptionMother.DatabaseTimeout(),
            CancellationToken.None);

        http.Response.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
    }

    [Test]
    public async Task find_the_transient_database_failure_wrapped_inside_another_exception()
    {
        DefaultHttpContext http = HttpContextMother.WithProblemDetails();

        await this._handler.TryHandleAsync(
            http,
            InfrastructureExceptionMother.WrappedTransientDatabase(),
            CancellationToken.None);

        http.Response.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
    }

    [Test]
    public async Task answer_500_for_a_database_failure_that_is_not_transient()
    {
        DefaultHttpContext http = HttpContextMother.WithProblemDetails();

        await this._handler.TryHandleAsync(
            http,
            InfrastructureExceptionMother.NonTransientDatabase(),
            CancellationToken.None);

        http.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        http.Response.Headers.RetryAfter.Should().BeEmpty();
        (await ReadBodyAsync(http)).GetProperty("code").GetString().Should().Be(ApiErrorCodes.UnexpectedError);
    }

    [Test]
    public async Task keep_the_status_of_a_bad_http_request_without_its_internal_message()
    {
        DefaultHttpContext http = HttpContextMother.WithProblemDetails();
        BadHttpRequestException exception = new(
            InfrastructureExceptionMother.INTERNAL_DETAIL,
            StatusCodes.Status415UnsupportedMediaType);

        bool handled = await this._handler.TryHandleAsync(http, exception, CancellationToken.None);

        handled.Should().BeTrue();
        http.Response.StatusCode.Should().Be(StatusCodes.Status415UnsupportedMediaType);
        JsonElement body = await ReadBodyAsync(http);
        body.GetProperty("code").GetString().Should().Be(ApiErrorCodes.BadRequest);
        body.GetRawText().Should().NotContain(InfrastructureExceptionMother.INTERNAL_DETAIL);
    }

    [Test]
    public async Task log_the_failure_of_the_writer_with_its_cause_and_the_trace_id_the_response_carries()
    {
        CapturingLoggerProvider logs = new();
        DefaultHttpContext http = HttpContextMother.WithProblemDetails();
        Exception exception = InfrastructureExceptionMother.WriterFailure();

        await HandlerLoggingTo(logs).TryHandleAsync(http, exception, CancellationToken.None);

        string traceId = (await ReadBodyAsync(http)).GetProperty("traceId").GetString()!;
        logs.Entries.Should().ContainSingle(entry => entry.Level == LogLevel.Error)
            .Which.Should().Match<CapturingLoggerProvider.LogEntry>(
                entry => ReferenceEquals(entry.Exception, exception) && entry.Message.Contains(traceId));
    }

    private static ApiExceptionHandler HandlerLoggingTo(CapturingLoggerProvider logs)
        => new(new LoggerFactory([logs]).CreateLogger<ApiExceptionHandler>());
}
