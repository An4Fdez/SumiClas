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
using static SumiClas.Api.Endpoints.EndpointsExtensions;

/// <summary>
/// The HTTP contract of the dashboard summary: a GET with the class and, optionally, the period, answering the summary
/// as JSON.
/// </summary>
[TestFixture]
public class DashboardEndpointsShould
{
    private const int REQUESTED_DAYS = 14;

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
        DashboardSummaryResponse summary = DashboardSummaryResponseMother.With(REQUESTED_DAYS);
        this._sender
            .Setup(sender => sender.Send(
                new DashboardSummaryFinderByClass.Query(classId, REQUESTED_DAYS),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = AuthenticatedClient(host);

        using HttpResponseMessage response = await client.GetAsync(
            $"{ApiPrefix}/dashboard/summary?classId={classId}&days={REQUESTED_DAYS}");

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
            .ReturnsAsync(DashboardSummaryResponseMother.With(SummaryPeriod.DefaultDays));
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = AuthenticatedClient(host);

        using HttpResponseMessage response = await client.GetAsync($"{ApiPrefix}/dashboard/summary?classId={classId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        this._sender.Verify(
            sender => sender.Send(
                new DashboardSummaryFinderByClass.Query(classId, null),
                It.IsAny<CancellationToken>()),
            Times.Once);
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
