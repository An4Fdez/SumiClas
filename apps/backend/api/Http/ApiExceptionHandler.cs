using Microsoft.AspNetCore.Diagnostics;
using Npgsql;
using SumiClas.Shared.Domain;
using System.Diagnostics;

namespace SumiClas.Api.Http;

/// <summary>
/// The single place where an exception thrown by a request becomes a response, always a ProblemDetails with a stable
/// <c>code</c> and the <c>traceId</c> (the same one the log line carries). It never answers with an internal message
/// or a stack trace.
/// <list type="bullet">
/// <item>A <see cref="DomainException"/> gets the status the endpoint declared for it (see
/// <see cref="DomainExceptionEndpointExtensions.Maps{TException}"/>, an override for the rare exception that deviates)
/// or, when it declared none, the one <see cref="DomainExceptionConvention"/> gives.</item>
/// <item>A <see cref="BadHttpRequestException"/> (malformed JSON, unsupported content type...) keeps its own 4xx.</item>
/// <item>A transient database failure is a 503 with <c>Retry-After</c>.</item>
/// <item>Anything else is a 500 logged as an error.</item>
/// </list>
/// </summary>
public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public const int RetryAfterSeconds = 10;

    private const string BadRequestMessage = "La petición no es válida.";
    private const string ServiceUnavailableMessage = "El servicio no está disponible ahora mismo. Inténtalo de nuevo en unos segundos.";
    private const string UnexpectedErrorMessage = "Ha ocurrido un error inesperado. Inténtalo de nuevo más tarde.";

    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception exception, CancellationToken ct)
    {
        if (exception is DomainException domain)
        {
            await HandleDomainAsync(http, domain);
            return true;
        }

        string traceId = TraceIdOf(http);

        if (exception is BadHttpRequestException badRequest)
        {
            logger.LogInformation(badRequest, "Malformed request {TraceId} answered with {Status}.", traceId, badRequest.StatusCode);
            await ApiProblem.Create(ApiErrorCodes.BadRequest, BadRequestMessage, badRequest.StatusCode).ExecuteAsync(http);
            return true;
        }

        if (IsTransientDatabaseFailure(exception))
        {
            logger.LogError(exception, "The database is not available {TraceId}.", traceId);
            http.Response.Headers.RetryAfter = RetryAfterSeconds.ToString();
            await ApiProblem.Create(
                ApiErrorCodes.ServiceUnavailable,
                ServiceUnavailableMessage,
                StatusCodes.Status503ServiceUnavailable).ExecuteAsync(http);
            return true;
        }

        logger.LogError(exception, "Unhandled exception {TraceId}.", traceId);
        await ApiProblem.Create(
            ApiErrorCodes.UnexpectedError,
            UnexpectedErrorMessage,
            StatusCodes.Status500InternalServerError).ExecuteAsync(http);
        return true;
    }

    private async Task HandleDomainAsync(HttpContext http, DomainException domain)
    {
        // The exception handler middleware clears the endpoint of the request before it calls the handlers;
        // the feature keeps the original one.
        Endpoint? endpoint = http.Features.Get<IExceptionHandlerFeature>()?.Endpoint ?? http.GetEndpoint();
        DomainExceptionStatus? declared = endpoint?.Metadata.GetOrderedMetadata<DomainExceptionStatus>()
            .FirstOrDefault(status => status.ExceptionType == domain.GetType());
        int statusCode = declared?.StatusCode ?? DomainExceptionConvention.StatusOf(domain);

        logger.LogInformation("Domain exception {Code} answered with {Status}.", domain.Code, statusCode);

        await DomainProblem.Create(domain, statusCode).ExecuteAsync(http);
    }

    // The same value ProblemDetails writes as traceId, so the user can quote it and it finds the log line.
    private static string TraceIdOf(HttpContext http) => Activity.Current?.Id ?? http.TraceIdentifier;

    // Entity Framework and the retry strategies wrap the Npgsql exception, so the whole chain is walked.
    private static bool IsTransientDatabaseFailure(Exception exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is NpgsqlException { IsTransient: true })
            {
                return true;
            }
        }

        return false;
    }
}
