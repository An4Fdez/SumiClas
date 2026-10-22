using Microsoft.AspNetCore.Diagnostics;
using SumiClas.Shared.Domain;

namespace SumiClas.Api.Http;

/// <summary>
/// Turns a <see cref="DomainException"/> into the standard ProblemDetails response, with its code in the
/// <c>code</c> extension. The status is the one the endpoint declared for that exception (see
/// <see cref="DomainExceptionEndpointExtensions.Maps{TException}"/>, an override for the rare exception that deviates)
/// or, when it declared none, the one <see cref="DomainExceptionConvention"/> gives.
/// </summary>
public sealed class DomainExceptionHandler(ILogger<DomainExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception exception, CancellationToken ct)
    {
        if (exception is not DomainException domain)
        {
            return false;
        }

        // The exception handler middleware clears the endpoint of the request before it calls the handlers;
        // the feature keeps the original one.
        Endpoint? endpoint = http.Features.Get<IExceptionHandlerFeature>()?.Endpoint ?? http.GetEndpoint();
        DomainExceptionStatus? declared = endpoint?.Metadata.GetOrderedMetadata<DomainExceptionStatus>()
            .FirstOrDefault(status => status.ExceptionType == domain.GetType());
        int statusCode = declared?.StatusCode ?? DomainExceptionConvention.StatusOf(domain);

        logger.LogInformation("Domain exception {Code} answered with {Status}.", domain.Code, statusCode);

        await DomainProblem.Create(domain, statusCode).ExecuteAsync(http);
        return true;
    }
}
