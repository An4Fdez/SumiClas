using MediatR;
using SumiClas.Dashboard.Application;

namespace SumiClas.Api.Endpoints;

public sealed class DashboardEndpoints : IEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder dashboard = app.MapGroup("/dashboard").WithTags("Dashboard");

        dashboard.MapGet("/summary", async (Guid classId, int? days, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new DashboardSummaryFinderByClass.Query(classId, days), ct)))
            .WithName("DashboardSummaryFinderByClass")
            .Produces<DashboardSummaryResponse>(StatusCodes.Status200OK);
    }
}
