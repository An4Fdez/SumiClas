using Microsoft.Extensions.FileProviders;
using SumiClas.Api.Endpoints;

namespace SumiClas.Api.Configuration;

public static class SpaWebApplicationExtensions
{
    private const string AssetsPrefix = "/assets";
    private const string IndexFile = "index.html";
    private const int SecondsPerMinute = 60;
    private const int MinutesPerHour = 60;
    private const int HoursPerDay = 24;
    private const int DaysPerYear = 365;
    private const int AssetsMaxAgeSeconds = SecondsPerMinute * MinutesPerHour * HoursPerDay * DaysPerYear;

    /// <summary>
    /// Serves the Vite build from wwwroot, with a fallback to <c>index.html</c> for the React Router routes.
    /// Call it right after the exception handler so the static files are answered before authentication.
    /// </summary>
    public static WebApplication UseSumiClasSpa(this WebApplication app)
    {
        app.UseDefaultFiles();
        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = context =>
            {
                context.Context.Response.Headers.CacheControl = context.Context.Request.Path.StartsWithSegments(AssetsPrefix)
                    ? $"public,max-age={AssetsMaxAgeSeconds},immutable"
                    : "no-cache"; // index.html must be revalidated so a new deployment reaches the users
            }
        });

        app.MapFallback(context =>
        {
            IFileInfo index = app.Environment.WebRootFileProvider.GetFileInfo(IndexFile);
            bool isPageRequest = HttpMethods.IsGet(context.Request.Method)
                                 && !context.Request.Path.StartsWithSegments(EndpointsExtensions.ApiPrefix);
            if (!isPageRequest || !index.Exists)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return Task.CompletedTask;
            }

            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.Headers.CacheControl = "no-cache";
            return context.Response.SendFileAsync(index);
        }).AllowAnonymous();

        return app;
    }
}
