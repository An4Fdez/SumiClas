namespace SumiClas.Tests.Api;

using AwesomeAssertions;
using MediatR;
using Moq;
using SumiClas.LostItems.Application;
using SumiClas.LostItems.Domain;
using SumiClas.Tests.Shared;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using static SumiClas.Api.Endpoints.EndpointsExtensions;

/// <summary>
/// The HTTP contract of the lost item claims: POST creates (201) with the id of the client, GET lists, and the two
/// PUT resolve (204) the claim of the route, each one reaching its own use case.
/// </summary>
[TestFixture]
public class LostItemClaimEndpointsShould
{
    private const string ALREADY_EXISTS_CODE = "LostItem.ClaimAlreadyExists";

    private readonly Mock<ISender> _sender = new();

    [SetUp]
    public void SetUp()
    {
        this._sender.Reset();
    }

    [Test]
    public async Task create_the_claim_of_the_route_item_with_the_id_and_message_of_the_body()
    {
        Guid lostItemId = GuidMother.Random();
        Guid claimId = GuidMother.Random();
        string message = StringMother.Random("detail");
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = AuthenticatedClient(host);
        using StringContent content = Json(RequestBodyMother.LostItemClaim(claimId, message));

        using HttpResponseMessage response = await client.PostAsync(
            $"{ApiPrefix}/lost-items/{lostItemId}/claims",
            content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        this.ShouldHaveSent(new LostItemClaimCreate.Command(claimId, lostItemId, message));
        this._sender.VerifyNoOtherCalls();
    }

    [Test]
    public async Task answer_409_with_the_claim_code_when_the_user_already_has_a_pending_claim()
    {
        this._sender
            .Setup(sender => sender.Send(It.IsAny<LostItemClaimCreate.Command>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new LostItemExceptions.ClaimAlreadyExists());
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = AuthenticatedClient(host);
        using StringContent content = Json(RequestBodyMother.LostItemClaim());

        using HttpResponseMessage response = await client.PostAsync(
            $"{ApiPrefix}/lost-items/{GuidMother.Random()}/claims",
            content);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await response.Content.ReadAsStringAsync()).Should().Contain(ALREADY_EXISTS_CODE);
    }

    [Test]
    public async Task list_the_claims_of_the_route_item_as_json()
    {
        Guid lostItemId = GuidMother.Random();
        IReadOnlyCollection<LostItemClaimResponse> claims =
        [
            new(
                GuidMother.Random(),
                GuidMother.Random(),
                StringMother.Random("name"),
                "Participant",
                StringMother.Random("detail"),
                "Accepted",
                StringMother.Random("pickup"),
                UnitTestCase.Now,
                true),
        ];
        this._sender
            .Setup(sender => sender.Send(
                new LostItemClaimFinderByItem.Query(lostItemId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(claims);
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = AuthenticatedClient(host);

        using HttpResponseMessage response = await client.GetAsync($"{ApiPrefix}/lost-items/{lostItemId}/claims");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<IReadOnlyCollection<LostItemClaimResponse>>())
            .Should().BeEquivalentTo(claims);
    }

    [Test]
    public async Task accept_the_claim_of_the_route_with_the_response_message_of_the_body()
    {
        Guid lostItemId = GuidMother.Random();
        Guid claimId = GuidMother.Random();
        string coordination = StringMother.Random("pickup");
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = AuthenticatedClient(host);
        using StringContent content = Json(RequestBodyMother.LostItemClaimAcceptance(coordination));

        using HttpResponseMessage response = await client.PutAsync(
            $"{ApiPrefix}/lost-items/{lostItemId}/claims/{claimId}/accept",
            content);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        this.ShouldHaveSent(new LostItemClaimAccept.Command(lostItemId, claimId, coordination));
        this._sender.VerifyNoOtherCalls();
    }

    [Test]
    public async Task accept_the_claim_without_response_message_when_the_request_has_no_body()
    {
        Guid lostItemId = GuidMother.Random();
        Guid claimId = GuidMother.Random();
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = AuthenticatedClient(host);

        using HttpResponseMessage response = await client.PutAsync(
            $"{ApiPrefix}/lost-items/{lostItemId}/claims/{claimId}/accept",
            null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        this.ShouldHaveSent(new LostItemClaimAccept.Command(lostItemId, claimId, null));
    }

    [Test]
    public async Task reject_the_claim_of_the_route()
    {
        Guid lostItemId = GuidMother.Random();
        Guid claimId = GuidMother.Random();
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = AuthenticatedClient(host);

        using HttpResponseMessage response = await client.PutAsync(
            $"{ApiPrefix}/lost-items/{lostItemId}/claims/{claimId}/reject",
            null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        this.ShouldHaveSent(new LostItemClaimReject.Command(lostItemId, claimId));
        this._sender.VerifyNoOtherCalls();
    }

    private static StringContent Json(string json) => new(json, Encoding.UTF8, "application/json");

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
