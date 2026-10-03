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
        var schoolYears = app.MapGroup("/school-years").WithTags("SchoolYears");

        schoolYears.MapPost("/", async (SchoolYearCreateRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(
                new SchoolYearCreate.Command(request.Id, request.SchoolId, request.Label, request.StartDate, request.EndDate), ct);

            return result.ToCreated();
        })
            .WithName("SchoolYearCreate")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        schoolYears.MapPost("/{schoolYearId:guid}/archive", async (Guid schoolYearId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new SchoolYearArchive.Command(schoolYearId), ct);

            return result.ToNoContent();
        })
            .WithName("SchoolYearArchive")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        schoolYears.MapGet("/", async (Guid schoolId, string? status, ISender sender, CancellationToken ct) =>
            (await sender.Send(new SchoolYearFinderBySchool.Query(schoolId, status), ct)).ToOk())
            .WithName("SchoolYearFinderBySchool")
            .Produces<IReadOnlyCollection<SchoolYearResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
