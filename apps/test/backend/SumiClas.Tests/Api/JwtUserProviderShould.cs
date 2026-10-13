namespace SumiClas.Tests.Api;

using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using SumiClas.Api.Http;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;

[TestFixture]
public class JwtUserProviderShould
{

    private readonly Mock<IHttpContextAccessor> _accessor = new();

    [SetUp]
    public void SetUp()
    {
        this._accessor.Reset();
    }

    [Test]
    public void return_the_user_id_when_the_sub_claim_is_valid()
    {
        Guid userId = GuidMother.Random();

        Result<Guid> result = this.ProviderFor(HttpContextMother.WithSubject(userId.ToString()))
            .GetCurrentUserId();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(userId);
    }

    [Test]
    public void be_unauthenticated_when_there_is_no_sub_claim()
    {
        Guid userId = GuidMother.Random();

        Result<Guid> result = this.ProviderFor(HttpContextMother.WithClaim("jti", userId.ToString()))
            .GetCurrentUserId();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthenticationErrors.Unauthenticated);
    }

    [Test]
    public void be_unauthenticated_when_there_is_no_http_context()
    {
        Result<Guid> result = this.ProviderFor(null).GetCurrentUserId();

        result.Error.Should().Be(AuthenticationErrors.Unauthenticated);
    }

    [TestCase("not-a-guid")]
    [TestCase("")]
    [TestCase("00000000-0000-0000-0000-000000000000")]
    public void be_unauthenticated_when_the_sub_claim_is_invalid(string sub)
    {
        Result<Guid> result = this.ProviderFor(HttpContextMother.WithSubject(sub)).GetCurrentUserId();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(AuthenticationErrors.Unauthenticated);
    }

    [Test]
    public void ignore_the_x_user_id_header()
    {
        DefaultHttpContext context = HttpContextMother.Empty();
        Guid userId = GuidMother.Random();
        context.Request.Headers["X-User-Id"] = userId.ToString();

        Result<Guid> result = this.ProviderFor(context).GetCurrentUserId();

        result.Error.Should().Be(AuthenticationErrors.Unauthenticated);
    }

    [Test]
    public void use_the_sub_when_a_different_x_user_id_header_is_present()
    {
        Guid userId = GuidMother.Random();
        DefaultHttpContext context = HttpContextMother.WithSubject(userId.ToString());
        context.Request.Headers["X-User-Id"] = GuidMother.Random().ToString();

        Result<Guid> result = this.ProviderFor(context).GetCurrentUserId();

        result.Value.Should().Be(userId);
    }

    private JwtUserProvider ProviderFor(HttpContext? context)
    {
        this._accessor.Setup(accessor => accessor.HttpContext).Returns(context);
        return new JwtUserProvider(this._accessor.Object);
    }
}
