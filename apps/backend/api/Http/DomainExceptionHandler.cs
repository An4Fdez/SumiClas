using Microsoft.AspNetCore.Diagnostics;
using SumiClas.Shared.Domain;

namespace SumiClas.Api.Http;

/// <summary>
/// Turns a <see cref="DomainException"/> into the standard ProblemDetails response, with the status the endpoint
/// declared for it (see <see cref="DomainExceptionEndpointExtensions.Maps{TException}"/>) and its code in the
/// <c>code</c> extension. A domain exception the endpoint did not declare is a programming mistake: it is logged as an
/// error and answered as a plain 500.
/// Temporary bridge: a <see cref="ResultFailureException"/> needs no declaration, it is answered with the status the
/// old <see cref="ResultExtensions"/> mapping gives its code.
/// </summary>
public sealed class DomainExceptionHandler(ILogger<DomainExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception exception, CancellationToken ct)
    {
        if (exception is not DomainException domain)
        {
            return false;
        }

        if (domain is ResultFailureException legacy)
        {
            logger.LogInformation("Failed result {Code} thrown by a use case answered.", legacy.Code);
            await legacy.Error.ToProblem().ExecuteAsync(http);
            return true;
        }

        // The exception handler middleware clears the endpoint of the request before it calls the handlers;
        // the feature keeps the original one.
        Endpoint? endpoint = http.Features.Get<IExceptionHandlerFeature>()?.Endpoint ?? http.GetEndpoint();
        DomainExceptionStatus? declared = endpoint?.Metadata
            .GetOrderedMetadata<DomainExceptionStatus>()
            .FirstOrDefault(status => status.ExceptionType == domain.GetType());

        if (declared is null)
        {
            logger.LogError(
                domain,
                "The domain exception {Code} ({Type}) is not declared by the endpoint {Endpoint}.",
                domain.Code,
                domain.GetType().FullName,
                endpoint?.DisplayName);

            await Results.Problem(statusCode: StatusCodes.Status500InternalServerError).ExecuteAsync(http);
            return true;
        }

        logger.LogInformation("Domain exception {Code} answered with {Status}.", domain.Code, declared.StatusCode);

        await Results.Problem(
            title: domain.Code,
            detail: domain.Message,
            statusCode: declared.StatusCode,
            extensions: new Dictionary<string, object?> { ["code"] = domain.Code }).ExecuteAsync(http);
        return true;
    }
}
