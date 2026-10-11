using MediatR;
using SumiClas.Api.Http;
using SumiClas.Schools.Application;

namespace SumiClas.Api.Endpoints;

public sealed class SchoolsEndpoints : IEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        var schools = app.MapGroup("/schools").WithTags("Schools");

        schools.MapGet("/{schoolId:guid}", async (Guid schoolId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new SchoolFinderById.Query(schoolId), ct)).ToOk())
            .WithName("SchoolFinderById")
            .Produces<SchoolResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        schools.MapGet("/", async (Guid? localityId, string? name, ISender sender, CancellationToken ct) =>
            (await sender.Send(new SchoolFinderByLocality.Query(localityId, name), ct)).ToOk())
            .WithName("SchoolFinderByLocality")
            .Produces<IReadOnlyCollection<SchoolResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
