namespace SumiClas.Tests.Api;

using AwesomeAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SumiClas.Api.Http;

[TestFixture]
public class DomainExceptionEndpointExtensionsShould
{
    [Test]
    public async Task declare_each_exception_with_its_own_status()
    {
        await using WebApplication app = WebApplication.CreateSlimBuilder().Build();
        app.MapGet("/sample", () => "sample")
            .Maps<SampleExceptions.NotFound>(StatusCodes.Status404NotFound)
            .Maps<SampleExceptions.Forbidden>(StatusCodes.Status403Forbidden);

        IReadOnlyList<DomainExceptionStatus> declared = EndpointOf(app).Metadata
            .GetOrderedMetadata<DomainExceptionStatus>();

        declared.Should().BeEquivalentTo(
        [
            new DomainExceptionStatus(typeof(SampleExceptions.NotFound), StatusCodes.Status404NotFound),
            new DomainExceptionStatus(typeof(SampleExceptions.Forbidden), StatusCodes.Status403Forbidden),
        ]);
    }

    [Test]
    public async Task document_the_status_as_a_problem_response()
    {
        await using WebApplication app = WebApplication.CreateSlimBuilder().Build();
        app.MapGet("/sample", () => "sample").Maps<SampleExceptions.NotFound>(StatusCodes.Status404NotFound);

        IProducesResponseTypeMetadata response = EndpointOf(app).Metadata
            .GetOrderedMetadata<IProducesResponseTypeMetadata>()
            .Single(metadata => metadata.StatusCode == StatusCodes.Status404NotFound);

        response.Type.Should().Be(typeof(ProblemDetails));
        response.ContentTypes.Should().Equal("application/problem+json");
    }

    [Test]
    public async Task document_a_status_once_when_two_exceptions_share_it()
    {
        await using WebApplication app = WebApplication.CreateSlimBuilder().Build();
        app.MapGet("/sample", () => "sample")
            .Maps<SampleExceptions.NotFound>(StatusCodes.Status404NotFound)
            .Maps<SampleExceptions.Forbidden>(StatusCodes.Status404NotFound);

        EndpointOf(app).Metadata
            .GetOrderedMetadata<IProducesResponseTypeMetadata>()
            .Count(metadata => metadata.StatusCode == StatusCodes.Status404NotFound)
            .Should().Be(1);
    }

    [Test]
    public async Task not_document_again_a_status_the_endpoint_already_documents()
    {
        await using WebApplication app = WebApplication.CreateSlimBuilder().Build();
        app.MapGet("/sample", () => "sample")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Maps<SampleExceptions.NotFound>(StatusCodes.Status404NotFound);

        EndpointOf(app).Metadata
            .GetOrderedMetadata<IProducesResponseTypeMetadata>()
            .Count(metadata => metadata.StatusCode == StatusCodes.Status404NotFound)
            .Should().Be(1);
    }

    private static Endpoint EndpointOf(WebApplication app)
        => ((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints).Single();
}
