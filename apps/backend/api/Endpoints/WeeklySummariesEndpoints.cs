using MediatR;
using SumiClas.WeeklySummaries.Application;

namespace SumiClas.Api.Endpoints;

public sealed class WeeklySummariesEndpoints : IEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder weeklySummaries = app.MapGroup("/weekly-summaries").WithTags("WeeklySummaries");

        weeklySummaries.MapPost("/", async (Guid classId, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new WeeklySummaryGenerate.Command(classId), ct)))
            .WithName("WeeklySummaryGenerate")
            .Produces<WeeklySummaryResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);
    }
}
