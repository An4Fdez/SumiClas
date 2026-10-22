using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace SumiClas.Api.Http;

/// <summary>
/// Documents in every operation the error responses a domain exception can produce, since the status of each one is
/// decided by <see cref="DomainExceptionConvention"/> and not declared by the endpoint: 400 and 401 always, 403 and 404
/// when the operation is not anonymous and 409 when it writes. They carry <see cref="ProblemDetails"/>, so the
/// generated client types the error with its <c>code</c>. A response the endpoint already declares is left alone.
/// </summary>
public sealed class ProblemResponsesTransformer : IOpenApiOperationTransformer
{
    private const string ProblemContentType = "application/problem+json";

    public async Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        bool anonymous = context.Description.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any();
        bool reads = string.Equals(context.Description.HttpMethod, HttpMethods.Get, StringComparison.OrdinalIgnoreCase);

        List<int> statuses = [StatusCodes.Status400BadRequest, StatusCodes.Status401Unauthorized];
        if (!anonymous)
        {
            statuses.Add(StatusCodes.Status403Forbidden);
            statuses.Add(StatusCodes.Status404NotFound);
        }

        if (!reads)
        {
            statuses.Add(StatusCodes.Status409Conflict);
        }

        IOpenApiSchema problem = await context.GetOrCreateSchemaAsync(typeof(ProblemDetails), null, cancellationToken);
        operation.Responses ??= [];
        foreach (int status in statuses)
        {
            string key = status.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (operation.Responses.TryGetValue(key, out IOpenApiResponse? existing) && existing.Content?.Count > 0)
            {
                continue;
            }

            operation.Responses[key] = new OpenApiResponse
            {
                Description = existing?.Description ?? Describe(status),
                Content = new Dictionary<string, OpenApiMediaType> { [ProblemContentType] = new() { Schema = problem } },
            };
        }
    }

    private static string Describe(int status) => status switch
    {
        StatusCodes.Status400BadRequest => "La petición no es válida.",
        StatusCodes.Status401Unauthorized => "No autenticado: falta el access token o no es válido.",
        StatusCodes.Status403Forbidden => "No tienes permiso para hacer esto.",
        StatusCodes.Status404NotFound => "No se ha encontrado lo que se pedía.",
        _ => "La operación entra en conflicto con datos que ya existen.",
    };
}
