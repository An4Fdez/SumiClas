using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using SumiClas.Shared.Domain;

namespace SumiClas.Api.Http;

public static class DomainExceptionEndpointExtensions
{
    private static readonly string[] ProblemContentTypes = ["application/problem+json"];

    /// <summary>
    /// Declares the HTTP status the endpoint answers when <typeparamref name="TException"/> comes out of its use case,
    /// and documents that response (once per status) in OpenAPI. An exception that is not declared by the endpoint
    /// that throws it is answered as a 500 by <see cref="DomainExceptionHandler"/>.
    /// </summary>
    public static RouteHandlerBuilder Maps<TException>(this RouteHandlerBuilder builder, int statusCode)
        where TException : DomainException
    {
        builder.Add(endpoint =>
        {
            endpoint.Metadata.Add(new DomainExceptionStatus(typeof(TException), statusCode));

            bool documented = endpoint.Metadata
                .OfType<IProducesResponseTypeMetadata>()
                .Any(metadata => metadata.StatusCode == statusCode);
            if (!documented)
            {
                endpoint.Metadata.Add(new ProducesResponseTypeMetadata(statusCode, typeof(ProblemDetails), ProblemContentTypes));
            }
        });

        return builder;
    }
}
