using System.ComponentModel.DataAnnotations;
using MediatR;
using SumiClas.Api.Http;
using SumiClas.SchoolYears.Application;

namespace SumiClas.Api.Endpoints;

public sealed record SchoolYearCreateRequest(
    Guid Id,
    Guid SchoolId,
    [StringLength(100)] string Label,
    DateOnly StartDate,
    DateOnly EndDate);

public sealed class SchoolYearsEndpoints : IEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        var schoolYears = app.MapGroup("/school-years");

        schoolYears.MapPost("/", async (SchoolYearCreateRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(
                new SchoolYearCreate.Command(request.Id, request.SchoolId, request.Label, request.StartDate, request.EndDate), ct);

            return result.IsSuccess
                ? Results.StatusCode(StatusCodes.Status201Created)
                : result.ToProblem();
        });
    }
}
