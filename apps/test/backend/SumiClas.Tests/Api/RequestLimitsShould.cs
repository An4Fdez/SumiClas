namespace SumiClas.Tests.Api;

using AwesomeAssertions;
using MediatR;
using Moq;
using SumiClas.ClassExpenses.Application;
using SumiClas.Sessions.Application;
using SumiClas.Tests.Shared;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using static SumiClas.Api.Endpoints.EndpointsExtensions;

/// <summary>
/// What the HTTP binding rejects before a use case runs: the money of an expense, the lengths that protect the password
/// hasher and the columns, and the personal data of a minor. Each limit is proved at the edge that is refused and at
/// the edge that is accepted, over the real API with the use cases faked.
/// </summary>
[TestFixture]
public class RequestLimitsShould
{
    private readonly Mock<ISender> _sender = new();

    [SetUp]
    public void SetUp()
    {
        this._sender.Reset();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task refuse_an_expense_whose_total_is_outside_the_allowed_range(bool aboveTheMaximum)
    {
        decimal total = aboveTheMaximum
            ? RequestBodyMother.EXPENSE_AMOUNT_MAX + 0.01m
            : RequestBodyMother.EXPENSE_AMOUNT_MIN - 0.01m;
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);

        using HttpResponseMessage response = await PostAsync(
            host,
            $"{ApiPrefix}/class-expenses",
            RequestBodyMother.Expense(totalAmount: total),
            authenticated: true);

        await this.ShouldHaveRefusedNamingTheField(response, "TotalAmount");
    }

    [Test]
    public async Task refuse_an_expense_whose_item_is_longer_than_allowed()
    {
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);

        using HttpResponseMessage response = await PostAsync(
            host,
            $"{ApiPrefix}/class-expenses",
            RequestBodyMother.Expense(item: RequestBodyMother.OfLength(RequestBodyMother.EXPENSE_ITEM_MAX + 1)),
            authenticated: true);

        await this.ShouldHaveRefusedNamingTheField(response, "Item");
    }

    [Test]
    public async Task refuse_an_expense_without_participants()
    {
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);

        using HttpResponseMessage response = await PostAsync(
            host,
            $"{ApiPrefix}/class-expenses",
            RequestBodyMother.Expense(withoutParticipants: true),
            authenticated: true);

        await this.ShouldHaveRefusedNamingTheField(response, "ParticipantIds");
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task accept_an_expense_whose_total_is_exactly_at_an_edge_of_the_range(bool atTheMaximum)
    {
        decimal total = atTheMaximum ? RequestBodyMother.EXPENSE_AMOUNT_MAX : RequestBodyMother.EXPENSE_AMOUNT_MIN;
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);

        using HttpResponseMessage response = await PostAsync(
            host,
            $"{ApiPrefix}/class-expenses",
            RequestBodyMother.Expense(totalAmount: total),
            authenticated: true);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        this.ShouldHaveReceived(typeof(ClassExpenseCreate.Command));
    }

    [TestCase($"{ApiPrefix}/auth/register")]
    [TestCase($"{ApiPrefix}/auth/login")]
    public async Task refuse_a_password_longer_than_the_hasher_accepts(string path)
    {
        string password = RequestBodyMother.OfLength(RequestBodyMother.PASSWORD_MAX + 1);
        string body = path == $"{ApiPrefix}/auth/register"
            ? RequestBodyMother.Registration(password: password)
            : RequestBodyMother.Login(password: password);
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);

        using HttpResponseMessage response = await PostAsync(host, path, body, authenticated: false);

        await this.ShouldHaveRefusedNamingTheField(response, "Password");
    }

    [Test]
    public async Task accept_a_password_of_exactly_the_maximum_length()
    {
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);

        using HttpResponseMessage response = await PostAsync(
            host,
            $"{ApiPrefix}/auth/login",
            RequestBodyMother.Login(password: RequestBodyMother.OfLength(RequestBodyMother.PASSWORD_MAX)),
            authenticated: false);

        response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest);
        this.ShouldHaveReceived(typeof(SessionCreate.Command));
    }

    [Test]
    public async Task refuse_an_email_longer_than_allowed()
    {
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);

        using HttpResponseMessage response = await PostAsync(
            host,
            $"{ApiPrefix}/auth/register",
            RequestBodyMother.Registration(email: RequestBodyMother.OfLength(RequestBodyMother.EMAIL_MAX + 1)),
            authenticated: false);

        await this.ShouldHaveRefusedNamingTheField(response, "Email");
    }

    [Test]
    public async Task refuse_a_photo_url_longer_than_the_column()
    {
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);

        using HttpResponseMessage response = await PostAsync(
            host,
            $"{ApiPrefix}/lost-items",
            RequestBodyMother.LostItem(photoUrl: RequestBodyMother.OfLength(RequestBodyMother.PHOTO_URL_MAX + 1)),
            authenticated: true);

        await this.ShouldHaveRefusedNamingTheField(response, "PhotoUrl");
    }

    [Test]
    public async Task refuse_the_first_name_of_a_student_longer_than_allowed()
    {
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);

        using HttpResponseMessage response = await PostAsync(
            host,
            $"{ApiPrefix}/students",
            RequestBodyMother.Student(firstName: RequestBodyMother.OfLength(RequestBodyMother.STUDENT_NAME_MAX + 1)),
            authenticated: true);

        await this.ShouldHaveRefusedNamingTheField(response, "FirstName");
    }

    private static async Task<HttpResponseMessage> PostAsync(
        EndpointsHost host,
        string path,
        string json,
        bool authenticated)
    {
        using HttpClient client = host.CreateClient();
        if (authenticated)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                host.AccessTokenOf(GuidMother.Random()));
        }

        using StringContent content = new(json, Encoding.UTF8, "application/json");
        return await client.PostAsync(path, content);
    }

    private static async Task<IReadOnlyList<string>> RefusedFieldsOf(HttpResponseMessage response)
    {
        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("errors").EnumerateObject().Select(field => field.Name).ToList();
    }

    private async Task ShouldHaveRefusedNamingTheField(HttpResponseMessage response, string field)
    {
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await RefusedFieldsOf(response)).Should()
            .Contain(name => string.Equals(name, field, StringComparison.OrdinalIgnoreCase));
        this._sender.VerifyNoOtherCalls();
    }

    private void ShouldHaveReceived(Type command)
        => this._sender.Invocations
            .Select(invocation => invocation.Arguments.FirstOrDefault()?.GetType())
            .Should().Contain(command);
}
