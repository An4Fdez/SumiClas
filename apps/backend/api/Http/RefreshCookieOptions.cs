namespace SumiClas.Api.Http;

public sealed class RefreshCookieOptions
{
    public const string SectionName = "Jwt:RefreshCookie";

    public string Name { get; init; } = "sumiclas_refresh";
    public string Path { get; init; } = "/api/auth";
    public bool Secure { get; init; } = true;
    public string SameSite { get; init; } = "Strict";
}
