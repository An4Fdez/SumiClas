namespace SumiClas.Api.Http;

/// <summary>The only place that touches the refresh cookie. Its value never goes to a response body or a log.</summary>
public static class RefreshCookie
{
    public static string? Read(HttpRequest request, RefreshCookieOptions options)
    {
        string? value = request.Cookies[options.Name];
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    public static void Write(HttpResponse response, string rawValue, DateTimeOffset expiresAt, DateTimeOffset now, RefreshCookieOptions options)
    {
        CookieOptions cookie = BaseOptions(options);
        cookie.Expires = expiresAt;
        cookie.MaxAge = expiresAt - now;
        response.Cookies.Append(options.Name, rawValue, cookie);
    }

    public static void Clear(HttpResponse response, RefreshCookieOptions options)
    {
        CookieOptions cookie = BaseOptions(options);
        cookie.Expires = DateTimeOffset.UnixEpoch;
        cookie.MaxAge = TimeSpan.Zero;
        response.Cookies.Append(options.Name, string.Empty, cookie);
    }

    public static bool TryParseSameSite(string? value, out SameSiteMode mode)
    {
        switch (value?.Trim().ToLowerInvariant())
        {
            case "strict":
                mode = SameSiteMode.Strict;
                return true;
            case "lax":
                mode = SameSiteMode.Lax;
                return true;
            case "none":
                mode = SameSiteMode.None;
                return true;
            default:
                mode = SameSiteMode.Unspecified;
                return false;
        }
    }

    private static CookieOptions BaseOptions(RefreshCookieOptions options)
    {
        TryParseSameSite(options.SameSite, out SameSiteMode sameSite);

        return new CookieOptions
        {
            HttpOnly = true,
            Secure = options.Secure,
            SameSite = sameSite,
            Path = options.Path,
        };
    }
}
