using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using SumiClas.Api.Http;

namespace SumiClas.Api.Configuration;

public static class OpenApiSecurityServiceCollectionExtensions
{
    private const string BearerSchemeName = "Bearer";

    /// <summary>
    /// Declares the Bearer scheme in every OpenAPI document (one per API version) and marks each non-anonymous operation
    /// with the security requirement and a 401 response. <c>ConfigureAll</c> reaches every named document, however it was registered.
    /// </summary>
    internal static IServiceCollection AddSumiClasOpenApiSecurity(this IServiceCollection services)
    {
        services.ConfigureAll<OpenApiOptions>(options =>
        {
            options.AddDocumentTransformer<BearerSecurityTransformer>();
            options.AddOperationTransformer<BearerSecurityTransformer>();
            options.AddDocumentTransformer<ProblemResponsesTransformer>();
            options.AddOperationTransformer<ProblemResponsesTransformer>();
        });

        return services;
    }

    private sealed class BearerSecurityTransformer : IOpenApiDocumentTransformer, IOpenApiOperationTransformer
    {
        public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes[BearerSchemeName] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Access token devuelto por POST /auth/login o POST /auth/refresh.",
            };

            return Task.CompletedTask;
        }

        public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
        {
            var isAnonymous = context.Description.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any();
            if (isAnonymous)
            {
                return Task.CompletedTask;
            }

            operation.Security ??= [];
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(BearerSchemeName, context.Document)] = [],
            });

            operation.Responses ??= [];
            operation.Responses.TryAdd("401", new OpenApiResponse { Description = "No autenticado: falta el access token o no es válido." });

            return Task.CompletedTask;
        }
    }
}
