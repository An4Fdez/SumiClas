using System.Globalization;
using System.Threading.RateLimiting;

namespace SumiClas.Api.Configuration;

public static class RateLimitingServiceCollectionExtensions
{
    /// <summary>Policy applied only to the anonymous /auth endpoints (register and login), which share one counter per client IP.</summary>
    public const string AuthPolicy = "auth";

    /// <summary>
    /// Fixed window per client IP. <c>RateLimiting:Auth:PermitLimit</c> requests per <c>RateLimiting:Auth:WindowSeconds</c>
    /// (default 10 per 60 s); over the limit the answer is 429 with a Retry-After header.
    /// The partition is the connection's remote IP, so behind a reverse proxy it only works once forwarded headers are configured.
    /// </summary>
    public static IServiceCollection AddSumiClasRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var permitLimit = configuration.GetValue("RateLimiting:Auth:PermitLimit", 10);
        var window = TimeSpan.FromSeconds(configuration.GetValue("RateLimiting:Auth:WindowSeconds", 60));

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(AuthPolicy, httpContext => RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window = window,
                    QueueLimit = 0,
                    AutoReplenishment = true,
                }));
            options.OnRejected = async (context, ct) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
                }

                await Results.Problem(
                    title: "Authentication.TooManyRequests",
                    detail: "Demasiados intentos. Espera un momento antes de volver a intentarlo.",
                    statusCode: StatusCodes.Status429TooManyRequests,
                    extensions: new Dictionary<string, object?> { ["code"] = "Authentication.TooManyRequests" })
                    .ExecuteAsync(context.HttpContext);
            };
        });

        return services;
    }
}
