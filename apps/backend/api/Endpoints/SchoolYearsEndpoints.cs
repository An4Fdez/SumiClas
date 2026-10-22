using System.ComponentModel.DataAnnotations;
using MediatR;
using SumiClas.Api.Http;
using SumiClas.SchoolYears.Application;
using SumiClas.SchoolYears.Domain;

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
            await sender.Send(
                new SchoolYearCreate.Command(request.Id, request.SchoolId, request.Label, request.StartDate, request.EndDate), ct);

            return Results.StatusCode(StatusCodes.Status201Created);
        })
            .WithName("SchoolYearCreate")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Maps<SchoolYearExceptions.IdRequired>(StatusCodes.Status400BadRequest)
            .Maps<SchoolYearExceptions.SchoolRequired>(StatusCodes.Status400BadRequest)
            .Maps<SchoolYearExceptions.LabelRequired>(StatusCodes.Status400BadRequest)
            .Maps<SchoolYearExceptions.InvalidDateRange>(StatusCodes.Status400BadRequest)
            .RequiresSchool()
            .ConflictsOnSave();

        schoolYears.MapPost("/{schoolYearId:guid}/archive", async (Guid schoolYearId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new SchoolYearArchive.Command(schoolYearId), ct);

            return Results.NoContent();
        })
            .WithName("SchoolYearArchive")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequiresSession()
            .RequiresClassRepresentative()
            .Maps<SchoolYearExceptions.NotFound>(StatusCodes.Status404NotFound)
            .Maps<SchoolYearExceptions.Archived>(StatusCodes.Status400BadRequest)
            .ConflictsOnSave();

        schoolYears.MapGet("/", async (Guid schoolId, string? status, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new SchoolYearFinderBySchool.Query(schoolId, status), ct)))
            .WithName("SchoolYearFinderBySchool")
            .Produces<IReadOnlyCollection<SchoolYearResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Maps<SchoolYearExceptions.InvalidStatus>(StatusCodes.Status400BadRequest)
            .RequiresSchool();
    }
}
