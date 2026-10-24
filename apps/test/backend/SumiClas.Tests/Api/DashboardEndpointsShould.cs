namespace SumiClas.Tests.Api;

using AwesomeAssertions;
using MediatR;
using Moq;
using SumiClas.Dashboard.Application;
using SumiClas.Shared.Domain;
using SumiClas.Tests.Dashboard.Application;
using SumiClas.Tests.Shared;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

/// <summary>
/// The HTTP contract of the dashboard summary: a GET with the class and, optionally, the period, answering the summary
/// as JSON, and a period out of range answering 400.
/// </summary>
[TestFixture]
public class DashboardEndpointsShould
{
    private const int REQUESTED_DAYS = 14;
    private const string INVALID_DAYS_CODE = "SummaryPeriod.InvalidDays";

    private readonly Mock<ISender> _sender = new();

    [SetUp]
    public void SetUp()
    {
        this._sender.Reset();
    }

    [Test]
    public async Task answer_the_summary_of_the_class_and_period_of_the_query_string()
    {
        Guid classId = GuidMother.Random();
        DashboardSummaryResponse summary = new(
            REQUESTED_DAYS,
            DashboardCountersMother.Announcements(),
            DashboardCountersMother.Assignments(),
            DashboardCountersMother.Expenses(),
            DashboardCountersMother.LostItems(),
            DashboardCountersMother.UpcomingEvents());
        this._sender
            .Setup(sender => sender.Send(
                new DashboardSummaryFinderByClass.Query(classId, REQUESTED_DAYS),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = AuthenticatedClient(host);

        using HttpResponseMessage response = await client.GetAsync(
            $"/dashboard/summary?classId={classId}&days={REQUESTED_DAYS}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<DashboardSummaryResponse>()).Should().BeEquivalentTo(summary);
    }

    [Test]
    public async Task leave_the_period_unset_when_the_query_string_has_none()
    {
        Guid classId = GuidMother.Random();
        this._sender
            .Setup(sender => sender.Send(
                It.IsAny<DashboardSummaryFinderByClass.Query>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DashboardSummaryResponse(
                SummaryPeriod.DefaultDays,
                DashboardCountersMother.Announcements(),
                DashboardCountersMother.Assignments(),
                DashboardCountersMother.Expenses(),
                DashboardCountersMother.LostItems(),
                DashboardCountersMother.UpcomingEvents()));
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = AuthenticatedClient(host);

        using HttpResponseMessage response = await client.GetAsync($"/dashboard/summary?classId={classId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        this._sender.Verify(
            sender => sender.Send(
                new DashboardSummaryFinderByClass.Query(classId, null),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task answer_400_with_the_period_code_when_the_period_is_out_of_range()
    {
        this._sender
            .Setup(sender => sender.Send(
                It.IsAny<DashboardSummaryFinderByClass.Query>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new SummaryPeriodExceptions.InvalidDays());
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = AuthenticatedClient(host);

        using HttpResponseMessage response = await client.GetAsync(
            $"/dashboard/summary?classId={GuidMother.Random()}&days={SummaryPeriod.MaxDays + 1}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain(INVALID_DAYS_CODE);
    }

    [Test]
    public async Task answer_400_without_reaching_the_use_case_when_the_class_is_not_a_guid()
    {
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = AuthenticatedClient(host);

        using HttpResponseMessage response = await client.GetAsync("/dashboard/summary?classId=not-a-guid");

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
