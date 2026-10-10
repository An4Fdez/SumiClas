using MediatR;
using SumiClas.Api.Http;
using SumiClas.Localities.Application;

namespace SumiClas.Api.Endpoints;

public sealed class LocalitiesEndpoints : IEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        var localities = app.MapGroup("/provinces/{provinceId:guid}/localities").WithTags("Localities");

        localities.MapGet("/", async (Guid provinceId, string? name, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new LocalityFinderByProvince.Query(provinceId, name), ct)))
            .WithName("LocalityFinderByProvince")
            .Produces<IReadOnlyCollection<LocalityResponse>>(StatusCodes.Status200OK);
    }
}
