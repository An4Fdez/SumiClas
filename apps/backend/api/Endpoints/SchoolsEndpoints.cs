using MediatR;
using SumiClas.Api.Http;
using SumiClas.Schools.Application;
using SumiClas.Schools.Domain;
using SumiClas.Shared.Domain;

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
            localityId is null || localityId == Guid.Empty
                ? Result.Failure(SchoolErrors.LocalityFilterRequired).ToProblem()
                : Results.Ok(await sender.Send(new SchoolFinderByLocality.Query(localityId.Value, name), ct)))
            .WithName("SchoolFinderByLocality")
            .Produces<IReadOnlyCollection<SchoolResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
