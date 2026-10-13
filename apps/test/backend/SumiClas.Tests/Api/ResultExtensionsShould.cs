namespace SumiClas.Tests.Api;

using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using SumiClas.Api.Http;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Shared;
using SumiClas.Tests.Shared.Domain;
using System.Text.Json;

[TestFixture]
public class ResultExtensionsShould : ApiUnitTestCase
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
    public async Task map_the_error_code_suffix_to_the_status_when_converting_a_failure_to_a_problem(
        string code,
        int expected)
    {
        string message = StringMother.Random("message");
        Result failure = Result.Failure(ErrorMother.With(code, message));

        (int status, JsonElement body) = await ExecuteAsync(failure.ToProblem());

        status.Should().Be(expected);
        body.GetProperty("code").GetString().Should().Be(code);
        body.GetProperty("status").GetInt32().Should().Be(expected);
        body.GetProperty("detail").GetString().Should().Be(message);
    }

    [Test]
    public void throw_when_converting_a_success_to_a_problem()
    {
        Func<IResult> act = () => Result.Success().ToProblem();

        act.Should().ThrowExactly<InvalidOperationException>();
    }

    [Test]
    public async Task answer_401_with_the_refresh_token_invalid_code_when_the_refresh_token_is_invalid()
    {
        (int status, JsonElement body) = await ExecuteAsync(ResultExtensions.RefreshTokenInvalidProblem());

        status.Should().Be(401);
        body.GetProperty("code").GetString().Should().Be("Authentication.RefreshTokenInvalid");
    }

    [Test]
    public async Task answer_401_with_the_unauthenticated_code_when_the_user_is_unauthenticated()
    {
        (int status, JsonElement body) = await ExecuteAsync(ResultExtensions.UnauthenticatedProblem());

        status.Should().Be(401);
        body.GetProperty("code").GetString().Should().Be("Authentication.Unauthenticated");
    }

    [Test]
    public async Task answer_403_with_the_forbidden_code_when_the_access_is_forbidden()
    {
        (int status, JsonElement body) = await ExecuteAsync(ResultExtensions.ForbiddenProblem());

        status.Should().Be(403);
        body.GetProperty("code").GetString().Should().Be("Authentication.Forbidden");
    }
}
