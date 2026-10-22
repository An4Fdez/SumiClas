using System.ComponentModel.DataAnnotations;
using MediatR;
using SumiClas.Api.Http;
using SumiClas.Subjects.Application;
using SumiClas.Subjects.Domain;

namespace SumiClas.Api.Endpoints;

public sealed record SubjectCreateRequest(Guid Id, Guid ClassId, [StringLength(100)] string Label);

public sealed class SubjectsEndpoints : IEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        var subjects = app.MapGroup("/subjects").WithTags("Subjects");

        subjects.MapPost("/", async (SubjectCreateRequest request, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new SubjectCreate.Command(request.Id, request.ClassId, request.Label), ct);

            return Results.StatusCode(StatusCodes.Status201Created);
        })
            .WithName("SubjectCreate")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Maps<SubjectExceptions.IdRequired>(StatusCodes.Status400BadRequest)
            .Maps<SubjectExceptions.ClassRequired>(StatusCodes.Status400BadRequest)
            .Maps<SubjectExceptions.LabelRequired>(StatusCodes.Status400BadRequest)
            .RequiresSession()
            .RequiresClass()
            .RequiresClassRepresentative()
            .RequiresActiveSchoolYear()
            .ConflictsOnSave();

        subjects.MapGet("/", async (Guid classId, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new SubjectFinderByClass.Query(classId), ct)))
            .WithName("SubjectFinderByClass")
            .Produces<IReadOnlyCollection<SubjectResponse>>(StatusCodes.Status200OK)
            .RequiresSession()
            .RequiresClass()
            .RequiresClassMembership();
    }
}
