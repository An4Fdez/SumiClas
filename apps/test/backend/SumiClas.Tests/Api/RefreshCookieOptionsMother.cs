namespace SumiClas.Tests.Api;

using SumiClas.Api.Http;

public static class RefreshCookieOptionsMother
{
    public static RefreshCookieOptions Create(string name, string path, bool secure, string sameSite)
        => new() { Name = name, Path = path, Secure = secure, SameSite = sameSite };

    public static RefreshCookieOptions Default() => Create("sumiclas_refresh", "/api/auth", true, "Strict");
}
