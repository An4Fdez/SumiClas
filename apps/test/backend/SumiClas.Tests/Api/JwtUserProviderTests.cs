using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;
using SumiClas.Api.Http;
using SumiClas.Shared.Domain;

namespace SumiClas.Tests.Api;

[TestFixture]
public class JwtUserProviderTests
{
    private static readonly Guid UserId = Guid.Parse("3f2b1c7e-8d4a-4c1e-9b5f-0a6d2e7c9b10");

    private static JwtUserProvider ProviderFor(HttpContext? context)
    {
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns(context);
        return new JwtUserProvider(accessor.Object);
    }

    private static DefaultHttpContext ContextWithClaims(params Claim[] claims)
    {
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
        return context;
    }

    [Test]
    public void GetCurrentUserId_WithAValidSubClaim_ReturnsTheUserId()
    {
        var result = ProviderFor(ContextWithClaims(new Claim("sub", UserId.ToString()))).GetCurrentUserId();

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(UserId));
    }

    [Test]
    public void GetCurrentUserId_WithoutASubClaim_IsUnauthenticated()
    {
        var result = ProviderFor(ContextWithClaims(new Claim("jti", UserId.ToString()))).GetCurrentUserId();

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(AuthenticationErrors.Unauthenticated));
    }

    [Test]
    public void GetCurrentUserId_WithoutAnHttpContext_IsUnauthenticated()
    {
        var result = ProviderFor(null).GetCurrentUserId();

        Assert.That(result.Error, Is.EqualTo(AuthenticationErrors.Unauthenticated));
    }

    [TestCase("not-a-guid")]
    [TestCase("")]
    [TestCase("00000000-0000-0000-0000-000000000000")]
    public void GetCurrentUserId_WithAnInvalidSubClaim_IsUnauthenticated(string sub)
    {
        var result = ProviderFor(ContextWithClaims(new Claim("sub", sub))).GetCurrentUserId();

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(AuthenticationErrors.Unauthenticated));
    }

    [Test]
    public void GetCurrentUserId_IgnoresTheXUserIdHeader()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Id"] = UserId.ToString();

        var result = ProviderFor(context).GetCurrentUserId();

        Assert.That(result.Error, Is.EqualTo(AuthenticationErrors.Unauthenticated));
    }

    [Test]
    public void GetCurrentUserId_WithASubAndADifferentXUserIdHeader_UsesTheSub()
    {
        var context = ContextWithClaims(new Claim("sub", UserId.ToString()));
        context.Request.Headers["X-User-Id"] = Guid.NewGuid().ToString();

        var result = ProviderFor(context).GetCurrentUserId();

        Assert.That(result.Value, Is.EqualTo(UserId));
    }
}
