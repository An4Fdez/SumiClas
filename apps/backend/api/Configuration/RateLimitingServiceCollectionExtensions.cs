using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace SumiClas.Api.Configuration;

public static class RateLimitingServiceCollectionExtensions
{
    /// <summary>Policy applied only to the anonymous /auth/register and /auth/login endpoints, which share one counter per client IP.</summary>
    public const string AuthPolicy = "auth";

    /// <summary>Policy applied only to /auth/refresh and /auth/logout, which share their own counter per client IP, separate from <see cref="AuthPolicy"/>.</summary>
    public const string AuthRefreshPolicy = "auth-refresh";

    /// <summary>
    /// Fixed window per client IP. <c>RateLimiting:Auth:PermitLimit</c> requests per <c>RateLimiting:Auth:WindowSeconds</c>
    /// (default 10 per 60 s) for register and login, and <c>RateLimiting:AuthRefresh:*</c> (default 30 per 60 s) for refresh and logout;
    /// over the limit the answer is 429 with a Retry-After header.
    /// The partition is the connection's remote IP, so behind a reverse proxy it only works once forwarded headers are configured.
    /// </summary>
    public static IServiceCollection AddSumiClasRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            AddFixedWindowByIp(options, AuthPolicy, configuration, "RateLimiting:Auth", defaultPermitLimit: 10);
            AddFixedWindowByIp(options, AuthRefreshPolicy, configuration, "RateLimiting:AuthRefresh", defaultPermitLimit: 30);
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

    private static void AddFixedWindowByIp(
        RateLimiterOptions options, string policyName, IConfiguration configuration, string section, int defaultPermitLimit)
    {
        var permitLimit = configuration.GetValue($"{section}:PermitLimit", defaultPermitLimit);
        var window = TimeSpan.FromSeconds(configuration.GetValue($"{section}:WindowSeconds", 60));

        options.AddPolicy(policyName, httpContext => RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = window,
                QueueLimit = 0,
                AutoReplenishment = true,
            }));
    }
}
