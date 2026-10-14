namespace SumiClas.Tests.Api;

using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SumiClas.Api.Configuration;
using SumiClas.Api.Http;
using SumiClas.Tests.Shared;

[TestFixture]
public class RefreshCookieShould : UnitTestCase
{
    [Test]
    public void set_a_http_only_secure_strict_cookie_with_the_configured_path_and_no_domain()
    {
        DefaultHttpContext context = HttpContextMother.Empty();
        string value = StringMother.Random("raw");

        RefreshCookie.Write(
            context.Response,
            value,
            Now + TimeSpan.FromDays(14),
            Now,
            RefreshCookieOptionsMother.Default());

        string header = SetCookieOf(context);
        header.Should().StartWith("sumiclas_refresh=" + value);
        header.Should().ContainEquivalentOf("httponly");
        header.Should().ContainEquivalentOf("secure");
        header.Should().ContainEquivalentOf("samesite=strict");
        header.Should().ContainEquivalentOf("path=/api/auth");
        header.Should().ContainEquivalentOf("max-age=1209600");
        header.Should().ContainEquivalentOf("expires=");
        header.Should().NotContainEquivalentOf("domain=");
    }

    [Test]
    public void use_the_name_secure_flag_same_site_and_path_from_the_options()
    {
        DefaultHttpContext context = HttpContextMother.Empty();
        string value = StringMother.Random("raw");
        string name = StringMother.Random("cookie");
        string path = StringMother.Random("path");
        RefreshCookieOptions options = RefreshCookieOptionsMother.Create(name, "/" + path, false, "lax");

        RefreshCookie.Write(context.Response, value, Now + TimeSpan.FromDays(1), Now, options);

        string header = SetCookieOf(context);
        header.Should().StartWith(name + "=" + value);
        header.Should().NotContainEquivalentOf("secure");
        header.Should().ContainEquivalentOf("samesite=lax");
        header.Should().ContainEquivalentOf("path=/" + path);
        header.Should().ContainEquivalentOf("max-age=86400");
    }

    [Test]
    public void expire_the_cookie_with_the_same_name_path_secure_and_same_site_when_it_is_cleared()
    {
        DefaultHttpContext context = HttpContextMother.Empty();

        RefreshCookie.Clear(context.Response, RefreshCookieOptionsMother.Default());

        string header = SetCookieOf(context);
        header.Should().StartWith("sumiclas_refresh=;");
        header.Should().ContainEquivalentOf("max-age=0");
        header.Should().ContainEquivalentOf("expires=Thu, 01 Jan 1970");
        header.Should().ContainEquivalentOf("path=/api/auth");
        header.Should().ContainEquivalentOf("secure");
        header.Should().ContainEquivalentOf("samesite=strict");
        header.Should().ContainEquivalentOf("httponly");
        header.Should().NotContainEquivalentOf("domain=");
    }

    [Test]
    public void return_the_cookie_value_by_the_configured_name()
    {
        DefaultHttpContext context = HttpContextMother.Empty();
        string value = StringMother.Random("raw");
        context.Request.Headers.Cookie = StringMother.Random("other") + "=1; sumiclas_refresh=" + value;

        RefreshCookie.Read(context.Request, RefreshCookieOptionsMother.Default()).Should().Be(value);
    }

    [Test]
    public void return_null_when_the_cookie_is_missing_or_blank()
    {
        DefaultHttpContext context = HttpContextMother.Empty();
        RefreshCookie.Read(context.Request, RefreshCookieOptionsMother.Default()).Should().BeNull();

        context.Request.Headers.Cookie = "sumiclas_refresh=";
        RefreshCookie.Read(context.Request, RefreshCookieOptionsMother.Default()).Should().BeNull();
    }

    [Test]
    public void succeed_validation_when_the_defaults_are_used()
    {
        Func<RefreshCookieOptions> act = () => Resolve(RefreshCookieSettingsMother.Empty()).Value;

        act.Should().NotThrow();
    }

    [TestCase("Name", "  ")]
    [TestCase("Path", "api/auth")]
    [TestCase("SameSite", "Unspecified")]
    [TestCase("SameSite", "banana")]
    public void fail_validation_when_a_value_is_invalid(string key, string value)
    {
        IOptions<RefreshCookieOptions> options = Resolve(RefreshCookieSettingsMother.Create(key, value));

        Func<RefreshCookieOptions> act = () => options.Value;

        act.Should().ThrowExactly<OptionsValidationException>();
    }

    [Test]
    public void fail_validation_when_same_site_is_none_without_secure()
    {
        IOptions<RefreshCookieOptions> options = Resolve(RefreshCookieSettingsMother.With("None", "false"));

        Func<RefreshCookieOptions> act = () => options.Value;

        act.Should().ThrowExactly<OptionsValidationException>().Which.Message.Should().Contain("Secure");
    }

    [Test]
    public void succeed_validation_when_same_site_is_none_with_secure()
    {
        IOptions<RefreshCookieOptions> options = Resolve(RefreshCookieSettingsMother.With("none", "true"));

        Func<RefreshCookieOptions> act = () => options.Value;

        act.Should().NotThrow();
    }

    private static string SetCookieOf(HttpContext context) => context.Response.Headers.SetCookie.ToString();

    private static IOptions<RefreshCookieOptions> Resolve(Dictionary<string, string?> values)
    {
        IConfiguration configuration = ConfigurationMother.Create(values);
        ServiceProvider provider = new ServiceCollection().AddSumiClasApiSecurity(configuration).BuildServiceProvider();
        return provider.GetRequiredService<IOptions<RefreshCookieOptions>>();
    }
}
