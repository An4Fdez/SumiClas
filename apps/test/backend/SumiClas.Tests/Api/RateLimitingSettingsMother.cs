namespace SumiClas.Tests.Api;

public static class RateLimitingSettingsMother
{
    public const int PERMIT_LIMIT = 2;
    public const int WINDOW_SECONDS = 3600;

    public static Dictionary<string, string?> WithTheSmallLimits()
        => new()
        {
            ["RateLimiting:Auth:PermitLimit"] = PERMIT_LIMIT.ToString(),
            ["RateLimiting:Auth:WindowSeconds"] = WINDOW_SECONDS.ToString(),
            ["RateLimiting:AuthRefresh:PermitLimit"] = PERMIT_LIMIT.ToString(),
            ["RateLimiting:AuthRefresh:WindowSeconds"] = WINDOW_SECONDS.ToString(),
        };
}
