namespace SumiClas.Tests.Api;

using SumiClas.Api.Http;

public static class RefreshCookieSettingsMother
{
    public static Dictionary<string, string?> Create(string key, string value)
        => new() { [RefreshCookieOptions.SectionName + ":" + key] = value };

    public static Dictionary<string, string?> Empty() => [];

    public static Dictionary<string, string?> With(string sameSite, string secure)
        => new()
        {
            [RefreshCookieOptions.SectionName + ":SameSite"] = sameSite,
            [RefreshCookieOptions.SectionName + ":Secure"] = secure,
        };
}
