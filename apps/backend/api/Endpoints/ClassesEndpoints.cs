using System.ComponentModel.DataAnnotations;
using MediatR;
using SumiClas.Api.Http;
using SumiClas.Classes.Application;

namespace SumiClas.Api.Endpoints;

public sealed record ClassCreateRequest(Guid Id, Guid SchoolYearId, [StringLength(100)] string Name);

public sealed record ClassRenameRequest([StringLength(100)] string Name);

public sealed class ClassesEndpoints : IEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder classes = app.MapGroup("/classes").WithTags("Classes");

        classes.MapPost("/", async (ClassCreateRequest request, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new ClassCreate.Command(request.Id, request.SchoolYearId, request.Name), ct);

            return Results.StatusCode(StatusCodes.Status201Created);
        })
            .WithName("ClassCreate")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        classes.MapPut("/{classId:guid}/name", async (Guid classId, ClassRenameRequest request, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new ClassRename.Command(classId, request.Name), ct);

            return Results.NoContent();
        })
            .WithName("ClassRename")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        classes.MapGet("/{classId:guid}", async (Guid classId, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new ClassFinderById.Query(classId), ct)))
            .WithName("ClassFinderById")
            .Produces<ClassResponse>(StatusCodes.Status200OK);

        classes.MapGet("/", async (Guid schoolYearId, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new ClassFinderBySchoolYear.Query(schoolYearId), ct)))
            .WithName("ClassFinderBySchoolYear")
            .Produces<IReadOnlyCollection<ClassResponse>>(StatusCodes.Status200OK);
    }
}
