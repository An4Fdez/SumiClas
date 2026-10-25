namespace SumiClas.Tests.Api;

using AwesomeAssertions;
using MediatR;
using Moq;
using SumiClas.Api.Http;
using SumiClas.Tests.Shared;
using SumiClas.Users.Domain;
using SumiClas.WeeklySummaries.Application;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

/// <summary>
/// The HTTP contract of the weekly summary: a POST with the class in the query string that answers the summary of the
/// current week as JSON, with a null text when the week has no news, and a 503 with its own code when the writer fails.
/// </summary>
[TestFixture]
public class WeeklySummariesEndpointsShould
{
    private const int DAYS_AFTER_MONDAY = 6;

    private static readonly DateOnly MONDAY = new(2026, 10, 5);
    private static readonly DateOnly SUNDAY = MONDAY.AddDays(DAYS_AFTER_MONDAY);

    private readonly Mock<ISender> _sender = new();

    [SetUp]
    public void SetUp()
    {
        this._sender.Reset();
    }

    [Test]
    public async Task answer_the_summary_of_the_class_of_the_query_string()
    {
        Guid classId = GuidMother.Random();
        WeeklySummaryResponse summary = new(MONDAY, SUNDAY, StringMother.Random("summary"), UnitTestCase.Now);
        this._sender
            .Setup(sender => sender.Send(new WeeklySummaryGenerate.Command(classId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = AuthenticatedClient(host);

        using HttpResponseMessage response = await client.PostAsync($"/weekly-summaries?classId={classId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<WeeklySummaryResponse>()).Should().Be(summary);
    }

    [Test]
    public async Task answer_a_week_without_news_with_no_text()
    {
        Guid classId = GuidMother.Random();
        WeeklySummaryResponse summary = new(MONDAY, SUNDAY, Content: null, GeneratedAt: null);
        this._sender
            .Setup(sender => sender.Send(new WeeklySummaryGenerate.Command(classId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = AuthenticatedClient(host);

        using HttpResponseMessage response = await client.PostAsync($"/weekly-summaries?classId={classId}", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<WeeklySummaryResponse>()).Should().Be(summary);
    }

    [Test]
    public async Task answer_503_with_retry_after_and_the_summary_code_when_the_writer_fails()
    {
        this._sender
            .Setup(sender => sender.Send(It.IsAny<WeeklySummaryGenerate.Command>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new WeeklySummaryWriterException(InfrastructureExceptionMother.INTERNAL_DETAIL));
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = AuthenticatedClient(host);

        using HttpResponseMessage response = await client.PostAsync(
            $"/weekly-summaries?classId={GuidMother.Random()}",
            null);

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        response.Headers.RetryAfter.Should().NotBeNull();
        string body = await response.Content.ReadAsStringAsync();
        body.Should().Contain(ApiErrorCodes.WeeklySummaryUnavailable);
        body.Should().NotContain(InfrastructureExceptionMother.INTERNAL_DETAIL);
    }

    [Test]
    public async Task answer_403_when_the_user_does_not_belong_to_the_class()
    {
        this._sender
            .Setup(sender => sender.Send(It.IsAny<WeeklySummaryGenerate.Command>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UserExceptions.ClassAccessForbidden());
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = AuthenticatedClient(host);

        using HttpResponseMessage response = await client.PostAsync(
            $"/weekly-summaries?classId={GuidMother.Random()}",
            null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task answer_400_without_reaching_the_use_case_when_the_class_is_not_a_guid()
    {
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = AuthenticatedClient(host);

        using HttpResponseMessage response = await client.PostAsync("/weekly-summaries?classId=not-a-guid", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        this._sender.VerifyNoOtherCalls();
    }

    [Test]
    public async Task answer_400_without_reaching_the_use_case_when_the_query_string_has_no_class()
    {
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = AuthenticatedClient(host);

        using HttpResponseMessage response = await client.PostAsync("/weekly-summaries", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        this._sender.VerifyNoOtherCalls();
    }

    private static HttpClient AuthenticatedClient(EndpointsHost host)
    {
        HttpClient client = host.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            host.AccessTokenOf(GuidMother.Random()));
        return client;
    }
}
