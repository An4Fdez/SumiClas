using MediatR;
using SumiClas.Api.Http;
using SumiClas.Schools.Application;
using SumiClas.Schools.Domain;

namespace SumiClas.Api.Endpoints;

public sealed class SchoolsEndpoints : IEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        var schools = app.MapGroup("/schools").WithTags("Schools");

        schools.MapGet("/{schoolId:guid}", async (Guid schoolId, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new SchoolFinderById.Query(schoolId), ct)))
            .WithName("SchoolFinderById")
            .Produces<SchoolResponse>(StatusCodes.Status200OK)
            .Maps<SchoolExceptions.NotFound>(StatusCodes.Status404NotFound);

        schools.MapGet("/", async (Guid? localityId, string? name, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new SchoolFinderByLocality.Query(localityId, name), ct)))
            .WithName("SchoolFinderByLocality")
            .Produces<IReadOnlyCollection<SchoolResponse>>(StatusCodes.Status200OK)
            .Maps<SchoolExceptions.LocalityFilterRequired>(StatusCodes.Status400BadRequest);
    }
}
