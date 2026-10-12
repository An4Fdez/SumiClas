using SumiClas.Api.Http;
using SumiClas.Shared.Domain;

namespace SumiClas.Tests.Api;

[TestFixture]
public class ResultExtensionsTests
{
    [TestCase("Authentication.Unauthenticated", 401)]
    [TestCase("Authentication.InvalidCredentials", 401)]
    [TestCase("Authentication.RefreshTokenInvalid", 401)]
    [TestCase("User.EmailAlreadyRegistered", 409)]
    [TestCase("Shared.AlreadyExists", 409)]
    [TestCase("Shared.ConcurrencyConflict", 409)]
    [TestCase("User.NotFound", 404)]
    [TestCase("Authentication.Forbidden", 403)]
    [TestCase("User.AssignForbidden", 403)]
    [TestCase("Authentication.PasswordTooWeak", 400)]
    [TestCase("School.LocalityFilterRequired", 400)]
    public async Task ToProblem_MapsTheErrorCodeSuffixToTheStatus(string code, int expected)
    {
        var (status, body) = await ApiTestHelpers.ExecuteAsync(Result.Failure(new Error(code, "mensaje")).ToProblem());

        Assert.That(status, Is.EqualTo(expected));
        Assert.That(body.GetProperty("code").GetString(), Is.EqualTo(code));
        Assert.That(body.GetProperty("status").GetInt32(), Is.EqualTo(expected));
        Assert.That(body.GetProperty("detail").GetString(), Is.EqualTo("mensaje"));
    }

    [Test]
    public void ToProblem_OnASuccess_Throws()
    {
        Assert.That(() => Result.Success().ToProblem(), Throws.InvalidOperationException);
    }

    [Test]
    public async Task RefreshTokenInvalidProblem_Is401WithTheRefreshTokenInvalidCode()
    {
        var (status, body) = await ApiTestHelpers.ExecuteAsync(ResultExtensions.RefreshTokenInvalidProblem());

        Assert.That(status, Is.EqualTo(401));
        Assert.That(body.GetProperty("code").GetString(), Is.EqualTo("Authentication.RefreshTokenInvalid"));
    }

    [Test]
    public async Task UnauthenticatedProblem_Is401WithTheUnauthenticatedCode()
    {
        var (status, body) = await ApiTestHelpers.ExecuteAsync(ResultExtensions.UnauthenticatedProblem());

        Assert.That(status, Is.EqualTo(401));
        Assert.That(body.GetProperty("code").GetString(), Is.EqualTo("Authentication.Unauthenticated"));
    }

    [Test]
    public async Task ForbiddenProblem_Is403WithTheForbiddenCode()
    {
        var (status, body) = await ApiTestHelpers.ExecuteAsync(ResultExtensions.ForbiddenProblem());

        Assert.That(status, Is.EqualTo(403));
        Assert.That(body.GetProperty("code").GetString(), Is.EqualTo("Authentication.Forbidden"));
    }
}
