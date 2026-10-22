using System.ComponentModel.DataAnnotations;
using MediatR;
using SumiClas.Api.Http;
using SumiClas.Assignments.Application;
using SumiClas.Assignments.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Subjects.Domain;

namespace SumiClas.Api.Endpoints;

public sealed record AssignmentCreateRequest(
    Guid Id,
    Guid ClassId,
    Guid SubjectId,
    [StringLength(200)] string Title,
    string? Description,
    DateOnly DueDate);

public sealed record AssignmentCommentAddRequest(string Text);

public sealed class AssignmentsEndpoints : IEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        var assignments = app.MapGroup("/assignments").WithTags("Assignments");

        assignments.MapPost("/", async (AssignmentCreateRequest request, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(
                new AssignmentCreate.Command(
                    request.Id,
                    request.ClassId,
                    request.SubjectId,
                    request.Title,
                    request.Description,
                    request.DueDate),
                ct);

            return Results.StatusCode(StatusCodes.Status201Created);
        })
            .WithName("AssignmentCreate")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Maps<AssignmentExceptions.IdRequired>(StatusCodes.Status400BadRequest)
            .Maps<AssignmentExceptions.ClassRequired>(StatusCodes.Status400BadRequest)
            .Maps<AssignmentExceptions.SubjectRequired>(StatusCodes.Status400BadRequest)
            .Maps<AssignmentExceptions.TitleRequired>(StatusCodes.Status400BadRequest)
            .Maps<AssignmentExceptions.AuthorRequired>(StatusCodes.Status400BadRequest)
            .Maps<AssignmentExceptions.SubjectNotInClass>(StatusCodes.Status400BadRequest)
            .Maps<SubjectExceptions.NotFound>(StatusCodes.Status404NotFound)
            .RequiresSession()
            .RequiresUser()
            .RequiresClass()
            .RequiresClassMembership()
            .RequiresActiveSchoolYear()
            .ConflictsOnSave();

        assignments.MapPost("/{assignmentId:guid}/comments", async (Guid assignmentId, AssignmentCommentAddRequest request, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new AssignmentCommentAdd.Command(assignmentId, request.Text), ct);

            return Results.StatusCode(StatusCodes.Status201Created);
        })
            .WithName("AssignmentCommentAdd")
            .Produces(StatusCodes.Status201Created)
            .Maps<AssignmentExceptions.NotFound>(StatusCodes.Status404NotFound)
            .Maps<AssignmentExceptions.CommentAuthorRequired>(StatusCodes.Status400BadRequest)
            .Maps<AssignmentExceptions.CommentTextRequired>(StatusCodes.Status400BadRequest)
            .RequiresSession()
            .RequiresUser()
            .RequiresClass()
            .RequiresClassMembership()
            .RequiresActiveSchoolYear()
            .ConflictsOnSave();

        assignments.MapGet("/", async (Guid classId, int? limit, string? cursor, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new AssignmentFinderByClass.Query(classId, limit, cursor), ct)))
            .WithName("AssignmentFinderByClass")
            .Produces<PagedResponse<AssignmentResponse>>(StatusCodes.Status200OK)
            .Maps<PaginationExceptions.InvalidLimit>(StatusCodes.Status400BadRequest)
            .Maps<PaginationExceptions.InvalidCursor>(StatusCodes.Status400BadRequest)
            .RequiresSession()
            .RequiresClass()
            .RequiresClassMembership();
    }
}
