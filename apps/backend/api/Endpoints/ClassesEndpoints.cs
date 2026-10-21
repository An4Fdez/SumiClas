using System.ComponentModel.DataAnnotations;
using MediatR;
using SumiClas.Api.Http;
using SumiClas.Classes.Application;
using SumiClas.Classes.Domain;

namespace SumiClas.Api.Endpoints;

public sealed record ClassCreateRequest(Guid Id, Guid SchoolYearId, [StringLength(100)] string Name);

public sealed record ClassRenameRequest([StringLength(100)] string Name);

public sealed class ClassesEndpoints : IEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        var classes = app.MapGroup("/classes").WithTags("Classes");

        classes.MapPost("/", async (ClassCreateRequest request, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new ClassCreate.Command(request.Id, request.SchoolYearId, request.Name), ct);

            return Results.StatusCode(StatusCodes.Status201Created);
        })
            .WithName("ClassCreate")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .Maps<ClassExceptions.IdRequired>(StatusCodes.Status400BadRequest)
            .Maps<ClassExceptions.SchoolYearRequired>(StatusCodes.Status400BadRequest)
            .Maps<ClassExceptions.NameRequired>(StatusCodes.Status400BadRequest)
            .Maps<ClassExceptions.CreatorRequired>(StatusCodes.Status400BadRequest)
            .RequiresActiveSchoolYear()
            .ConflictsOnSave();

        classes.MapPut("/{classId:guid}/name", async (Guid classId, ClassRenameRequest request, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new ClassRename.Command(classId, request.Name), ct);

            return Results.NoContent();
        })
            .WithName("ClassRename")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Maps<ClassExceptions.NameRequired>(StatusCodes.Status400BadRequest)
            .RequiresClass()
            .RequiresActiveSchoolYear()
            .ConflictsOnSave();

        classes.MapGet("/{classId:guid}", async (Guid classId, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new ClassFinderById.Query(classId), ct)))
            .WithName("ClassFinderById")
            .Produces<ClassResponse>(StatusCodes.Status200OK)
            .RequiresClass();

        classes.MapGet("/", async (Guid schoolYearId, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new ClassFinderBySchoolYear.Query(schoolYearId), ct)))
            .WithName("ClassFinderBySchoolYear")
            .Produces<IReadOnlyCollection<ClassResponse>>(StatusCodes.Status200OK);
    }
}
