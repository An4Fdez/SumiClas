namespace SumiClas.Tests.Api;

using AwesomeAssertions;
using MediatR;
using Moq;
using SumiClas.Assignments.Application;
using SumiClas.Tests.Shared;
using System.Net;
using System.Net.Http.Headers;
using static SumiClas.Api.Endpoints.EndpointsExtensions;

/// <summary>
/// The HTTP contract of the done mark: PUT marks and DELETE unmarks the assignment of the route, both answering 204
/// with no body, each one reaching its own use case.
/// </summary>
[TestFixture]
public class AssignmentDoneEndpointsShould
{
    private readonly Mock<ISender> _sender = new();

    [SetUp]
    public void SetUp()
    {
        this._sender.Reset();
    }

    [Test]
    public async Task mark_the_assignment_of_the_route_as_done_with_a_put()
    {
        Guid assignmentId = GuidMother.Random();
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = AuthenticatedClient(host);

        using HttpResponseMessage response = await client.PutAsync(
            $"{ApiPrefix}/assignments/{assignmentId}/done", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        this.ShouldHaveSent(new AssignmentDoneMark.Command(assignmentId));
        this._sender.VerifyNoOtherCalls();
    }

    [Test]
    public async Task unmark_the_assignment_of_the_route_with_a_delete()
    {
        Guid assignmentId = GuidMother.Random();
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = AuthenticatedClient(host);

        using HttpResponseMessage response = await client.DeleteAsync($"{ApiPrefix}/assignments/{assignmentId}/done");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        this.ShouldHaveSent(new AssignmentDoneUnmark.Command(assignmentId));
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

    private void ShouldHaveSent(IRequest command)
        => this._sender.Verify(sender => sender.Send(command, It.IsAny<CancellationToken>()), Times.Once);
}
