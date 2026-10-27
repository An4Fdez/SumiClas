namespace SumiClas.Tests.Api;

using AwesomeAssertions;
using MediatR;
using Moq;
using SumiClas.Announcements.Application;
using SumiClas.Shared.Domain;
using SumiClas.Students.Application;
using SumiClas.Tests.Shared;
using SumiClas.Users.Application;
using SumiClas.Users.Domain;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using static SumiClas.Api.Endpoints.EndpointsExtensions;

/// <summary>
/// The refusals that protect families and minors, as the client really sees them: a use case refuses with a domain
/// exception and the real API answers with the right status and the stable code the frontend branches on. The use
/// cases are faked; the rules behind each refusal are proved in their own tests.
/// </summary>
[TestFixture]
public class SecurityErrorsShould
{
    private readonly Mock<ISender> _sender = new();

    [SetUp]
    public void SetUp()
    {
        this._sender.Reset();
    }

    [Test]
    public async Task answer_403_when_a_family_reads_the_announcements_of_a_class_it_does_not_belong_to()
    {
        this.ShouldRefuseReadingAnnouncements(new UserExceptions.ClassAccessForbidden());
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = AuthenticatedClient(host);

        using HttpResponseMessage response = await client.GetAsync(
            $"{ApiPrefix}/announcements?classId={GuidMother.Random()}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await CodeOf(response)).Should().Be("User.ClassAccessForbidden");
        this.ShouldHaveReceived(typeof(AnnouncementFinderByClass.Query));
    }

    [Test]
    public async Task answer_403_when_someone_who_is_not_from_the_class_asks_who_its_families_are()
    {
        this.ShouldRefuseListingMembers(new UserExceptions.ClassMembersForbidden());
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = AuthenticatedClient(host);

        using HttpResponseMessage response = await client.GetAsync(
            $"{ApiPrefix}/classes/{GuidMother.Random()}/members");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await CodeOf(response)).Should().Be("User.ClassMembersForbidden");
        this.ShouldHaveReceived(typeof(UserClassMemberFinderByClass.Query));
    }

    [Test]
    public async Task answer_403_when_someone_publishes_an_announcement_to_the_whole_school()
    {
        this.ShouldRefusePublishingAnnouncements(new UserExceptions.SchoolPublishForbidden());
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = AuthenticatedClient(host);

        using HttpResponseMessage response = await PostAsync(
            client,
            $"{ApiPrefix}/announcements",
            RequestBodyMother.Announcement("School"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await CodeOf(response)).Should().Be("User.SchoolPublishForbidden");
        this.ShouldHaveReceived(typeof(AnnouncementCreate.Command));
    }

    [Test]
    public async Task answer_409_when_registering_a_student_whose_id_already_exists()
    {
        this.ShouldRefuseRegisteringStudents(new SharedExceptions.AlreadyExists());
        await using EndpointsHost host = await EndpointsHost.StartAsync(this._sender.Object);
        using HttpClient client = AuthenticatedClient(host);

        using HttpResponseMessage response = await PostAsync(
            client, $"{ApiPrefix}/students", RequestBodyMother.Student());

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await CodeOf(response)).Should().Be("Shared.AlreadyExists");
        this.ShouldHaveReceived(typeof(StudentRegister.Command));
    }

    private static HttpClient AuthenticatedClient(EndpointsHost host)
    {
        HttpClient client = host.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            host.AccessTokenOf(GuidMother.Random()));
        return client;
    }

    private static async Task<HttpResponseMessage> PostAsync(HttpClient client, string path, string json)
    {
        using StringContent content = new(json, Encoding.UTF8, "application/json");
        return await client.PostAsync(path, content);
    }

    private static async Task<string?> CodeOf(HttpResponseMessage response)
    {
        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("code").GetString();
    }

    private void ShouldHaveReceived(Type command)
        => this._sender.Invocations
            .Select(invocation => invocation.Arguments.FirstOrDefault()?.GetType())
            .Should().Contain(command);

    private void ShouldRefuseReadingAnnouncements(Exception exception)
        => this._sender
            .Setup(sender => sender.Send(It.IsAny<AnnouncementFinderByClass.Query>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

    private void ShouldRefuseListingMembers(Exception exception)
        => this._sender
            .Setup(sender => sender.Send(It.IsAny<UserClassMemberFinderByClass.Query>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

    private void ShouldRefusePublishingAnnouncements(Exception exception)
        => this._sender
            .Setup(sender => sender.Send(It.IsAny<AnnouncementCreate.Command>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

    private void ShouldRefuseRegisteringStudents(Exception exception)
        => this._sender
            .Setup(sender => sender.Send(It.IsAny<StudentRegister.Command>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
}
