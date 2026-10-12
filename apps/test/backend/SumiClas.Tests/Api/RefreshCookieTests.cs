using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SumiClas.Api.Configuration;
using SumiClas.Api.Http;

namespace SumiClas.Tests.Api;

[TestFixture]
public class RefreshCookieTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 10, 12, 0, 0, TimeSpan.Zero);

    private static RefreshCookieOptions Options(RefreshCookieOptions? options = null) => options ?? new RefreshCookieOptions();

    private static string SetCookieOf(HttpContext context) => context.Response.Headers.SetCookie.ToString();

    [Test]
    public void Write_SetsAHttpOnlySecureStrictCookieWithTheConfiguredPathAndNoDomain()
    {
        var context = new DefaultHttpContext();

        RefreshCookie.Write(context.Response, "raw-value", Now + TimeSpan.FromDays(14), Now, Options());

        var header = SetCookieOf(context);
        Assert.That(header, Does.StartWith("sumiclas_refresh=raw-value"));
        Assert.That(header, Does.Contain("httponly").IgnoreCase);
        Assert.That(header, Does.Contain("secure").IgnoreCase);
        Assert.That(header, Does.Contain("samesite=strict").IgnoreCase);
        Assert.That(header, Does.Contain("path=/api/auth").IgnoreCase);
        Assert.That(header, Does.Contain("max-age=1209600").IgnoreCase);
        Assert.That(header, Does.Contain("expires=").IgnoreCase);
        Assert.That(header, Does.Not.Contain("domain=").IgnoreCase);
    }

    [Test]
    public void Write_UsesTheNameSecureFlagSameSiteAndPathFromTheOptions()
    {
        var context = new DefaultHttpContext();
        var options = new RefreshCookieOptions { Name = "other", Secure = false, SameSite = "lax", Path = "/auth" };

        RefreshCookie.Write(context.Response, "v", Now + TimeSpan.FromDays(1), Now, options);

        var header = SetCookieOf(context);
        Assert.That(header, Does.StartWith("other=v"));
        Assert.That(header, Does.Not.Contain("secure").IgnoreCase);
        Assert.That(header, Does.Contain("samesite=lax").IgnoreCase);
        Assert.That(header, Does.Contain("path=/auth").IgnoreCase);
        Assert.That(header, Does.Contain("max-age=86400").IgnoreCase);
    }

    [Test]
    public void Clear_ExpiresTheCookieWithTheSameNamePathSecureAndSameSite()
    {
        var context = new DefaultHttpContext();

        RefreshCookie.Clear(context.Response, Options());

        var header = SetCookieOf(context);
        Assert.That(header, Does.StartWith("sumiclas_refresh=;"));
        Assert.That(header, Does.Contain("max-age=0").IgnoreCase);
        Assert.That(header, Does.Contain("expires=Thu, 01 Jan 1970").IgnoreCase);
        Assert.That(header, Does.Contain("path=/api/auth").IgnoreCase);
        Assert.That(header, Does.Contain("secure").IgnoreCase);
        Assert.That(header, Does.Contain("samesite=strict").IgnoreCase);
        Assert.That(header, Does.Contain("httponly").IgnoreCase);
        Assert.That(header, Does.Not.Contain("domain=").IgnoreCase);
    }

    [Test]
    public void Read_ReturnsTheCookieValueByConfiguredName()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.Cookie = "other=1; sumiclas_refresh=the-token";

        Assert.That(RefreshCookie.Read(context.Request, Options()), Is.EqualTo("the-token"));
    }

    [Test]
    public void Read_WithoutTheCookieOrWithABlankOne_ReturnsNull()
    {
        var context = new DefaultHttpContext();
        Assert.That(RefreshCookie.Read(context.Request, Options()), Is.Null);

        context.Request.Headers.Cookie = "sumiclas_refresh=";
        Assert.That(RefreshCookie.Read(context.Request, Options()), Is.Null);
    }

    private static IOptions<RefreshCookieOptions> Resolve(Dictionary<string, string?> values)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        var provider = new ServiceCollection().AddSumiClasApiSecurity(configuration).BuildServiceProvider();
        return provider.GetRequiredService<IOptions<RefreshCookieOptions>>();
    }

    [Test]
    public void Validation_WithDefaults_Succeeds()
    {
        Assert.That(() => Resolve([]).Value, Throws.Nothing);
    }

    [TestCase("Name", "  ")]
    [TestCase("Path", "api/auth")]
    [TestCase("SameSite", "Unspecified")]
    [TestCase("SameSite", "banana")]
    public void Validation_WithAnInvalidValue_Fails(string key, string value)
    {
        var options = Resolve(new() { [$"Jwt:RefreshCookie:{key}"] = value });

        Assert.That(() => options.Value, Throws.TypeOf<OptionsValidationException>());
    }

    [Test]
    public void Validation_SameSiteNoneWithoutSecure_Fails()
    {
        var options = Resolve(new() { ["Jwt:RefreshCookie:SameSite"] = "None", ["Jwt:RefreshCookie:Secure"] = "false" });

        Assert.That(() => options.Value, Throws.TypeOf<OptionsValidationException>().With.Message.Contains("Secure"));
    }

    [Test]
    public void Validation_SameSiteNoneWithSecure_Succeeds()
    {
        var options = Resolve(new() { ["Jwt:RefreshCookie:SameSite"] = "none", ["Jwt:RefreshCookie:Secure"] = "true" });

        Assert.That(() => options.Value, Throws.Nothing);
    }
}
