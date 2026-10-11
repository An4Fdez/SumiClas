namespace SumiClas.Authentication.Application;

public sealed class RefreshCookieOptions
{
    public const string SectionName = "Jwt:RefreshCookie";

    public string Name { get; set; } = "sumiclas_refresh";
    public string Path { get; set; } = "/api/auth";
    public bool Secure { get; set; } = true;
    public string SameSite { get; set; } = "Strict";
}
