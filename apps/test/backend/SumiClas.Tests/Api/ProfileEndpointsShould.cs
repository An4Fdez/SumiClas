namespace SumiClas.Tests.Api;

using AwesomeAssertions;
using MediatR;
using Moq;
using SumiClas.Students.Application;
using SumiClas.Tests.Shared;
using SumiClas.Users.Application;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using static SumiClas.Api.Endpoints.EndpointsExtensions;

/// <summary>
/// The HTTP contract of the profile edits: the current user's name and password, and the name of a student of the
/// route. Each answers 204 with no body, refuses what is longer than its column or the password hasher allows, and the
/// password change shares the brute-force brake of the authentication endpoints.
/// </summary>
[TestFixture]
public class ProfileEndpointsShould
{
    private const string NAME = "Lucia";
    private const string SURNAME = "Perez";
    private const string CURRENT_PASSWORD = "current-password";
    private const string NEW_PASSWORD = "new-password";

    private readonly Mock<ISender> _sender = new();

    [SetUp]
    public void SetUp()
    {
        this._sender.Reset();
    }

    [Test]
    public async Task change_the_name_of_the_current_user_with_a_put_to_me()
    {
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = AuthenticatedClient(host);

        using HttpResponseMessage response = await PutAsync(client, $"{ApiPrefix}/users/me", RequestBodyMother.UserName(NAME));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        this.ShouldHaveSent(new UserNameChange.Command(NAME));
        this._sender.VerifyNoOtherCalls();
    }

    [Test]
    public async Task change_the_password_of_the_current_user_with_a_put_to_me_password()
    {
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = AuthenticatedClient(host);

        using HttpResponseMessage response = await PutAsync(
            client,
            $"{ApiPrefix}/users/me/password",
            RequestBodyMother.PasswordChange(CURRENT_PASSWORD, NEW_PASSWORD));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        this.ShouldHaveSent(new UserPasswordChange.Command(CURRENT_PASSWORD, NEW_PASSWORD));
        this._sender.VerifyNoOtherCalls();
    }

    [Test]
    public async Task change_the_name_of_the_student_of_the_route()
    {
        Guid studentId = GuidMother.Random();
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = AuthenticatedClient(host);

        using HttpResponseMessage response = await PutAsync(
            client,
            $"{ApiPrefix}/students/{studentId}",
            RequestBodyMother.StudentName(NAME));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        this._sender.Invocations.Should().ContainSingle()
            .Which.Arguments[0].Should().BeOfType<StudentNameChange.Command>()
            .Which.Should().Match<StudentNameChange.Command>(command =>
                command.StudentId == studentId && command.FirstName == NAME && command.SecondSurname == null);
    }

    [Test]
    public async Task refuse_a_user_name_longer_than_its_column()
    {
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = AuthenticatedClient(host);

        using HttpResponseMessage response = await PutAsync(
            client,
            $"{ApiPrefix}/users/me",
            RequestBodyMother.UserName(RequestBodyMother.OfLength(RequestBodyMother.USER_NAME_MAX + 1)));

        await this.ShouldHaveRefusedNamingTheField(response, "FullName");
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task refuse_a_password_longer_than_the_hasher_allows(bool theNewOne)
    {
        string tooLong = RequestBodyMother.OfLength(RequestBodyMother.PASSWORD_MAX + 1);
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = AuthenticatedClient(host);

        using HttpResponseMessage response = await PutAsync(
            client,
            $"{ApiPrefix}/users/me/password",
            theNewOne
                ? RequestBodyMother.PasswordChange(newPassword: tooLong)
                : RequestBodyMother.PasswordChange(currentPassword: tooLong));

        await this.ShouldHaveRefusedNamingTheField(response, theNewOne ? "NewPassword" : "CurrentPassword");
    }

    [Test]
    public async Task refuse_a_student_first_name_longer_than_its_column()
    {
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = AuthenticatedClient(host);

        using HttpResponseMessage response = await PutAsync(
            client,
            $"{ApiPrefix}/students/{GuidMother.Random()}",
            RequestBodyMother.StudentName(RequestBodyMother.OfLength(RequestBodyMother.STUDENT_NAME_MAX + 1)));

        await this.ShouldHaveRefusedNamingTheField(response, "FirstName");
    }

    [Test]
    public async Task limit_the_password_change_attempts_and_then_answer_429_with_retry_after()
    {
        await using EndpointsHost host = await EndpointsHost.StartAsync(
            this._sender.Object,
            RateLimitingSettingsMother.WithTheSmallLimits());
        using HttpClient client = AuthenticatedClient(host);
        for (int attempt = 1; attempt <= RateLimitingSettingsMother.PERMIT_LIMIT; attempt++)
        {
            using HttpResponseMessage allowed = await PutAsync(
                client, $"{ApiPrefix}/users/me/password", RequestBodyMother.PasswordChange());
            allowed.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        using HttpResponseMessage refused = await PutAsync(
            client, $"{ApiPrefix}/users/me/password", RequestBodyMother.PasswordChange());

        refused.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        refused.Headers.RetryAfter.Should().NotBeNull();
    }

    private static HttpClient AuthenticatedClient(EndpointsHost host)
    {
        HttpClient client = host.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            host.AccessTokenOf(GuidMother.Random()));
        return client;
    }

    private static async Task<HttpResponseMessage> PutAsync(HttpClient client, string path, string json)
    {
        using StringContent content = new(json, Encoding.UTF8, "application/json");
        return await client.PutAsync(path, content);
    }

    private async Task ShouldHaveRefusedNamingTheField(HttpResponseMessage response, string field)
    {
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        document.RootElement.GetProperty("errors").EnumerateObject().Select(error => error.Name).Should()
            .Contain(name => string.Equals(name, field, StringComparison.OrdinalIgnoreCase));
        this._sender.VerifyNoOtherCalls();
    }

    private void ShouldHaveSent(IRequest command)
        => this._sender.Verify(sender => sender.Send(command, It.IsAny<CancellationToken>()), Times.Once);
}
