using MediatR;
using SumiClas.Api.Http;
using SumiClas.Schools.Application;

namespace SumiClas.Api.Endpoints;

public sealed class SchoolsEndpoints : IEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder schools = app.MapGroup("/schools").WithTags("Schools");

        schools.MapGet("/{schoolId:guid}", async (Guid schoolId, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new SchoolFinderById.Query(schoolId), ct)))
            .WithName("SchoolFinderById")
            .Produces<SchoolResponse>(StatusCodes.Status200OK);

        schools.MapGet("/", async (Guid? localityId, string? name, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new SchoolFinderByLocality.Query(localityId, name), ct)))
            .WithName("SchoolFinderByLocality")
            .Produces<IReadOnlyCollection<SchoolResponse>>(StatusCodes.Status200OK);
    }
}
