using MediatR;
using SumiClas.Api.Http;
using SumiClas.Provinces.Application;

namespace SumiClas.Api.Endpoints;

public sealed class ProvincesEndpoints : IEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder provinces = app.MapGroup("/provinces").WithTags("Provinces");

        provinces.MapGet("/", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new ProvinceFinderAll.Query(), ct)))
            .WithName("ProvinceFinderAll")
            .Produces<IReadOnlyCollection<ProvinceResponse>>(StatusCodes.Status200OK);
    }
}
