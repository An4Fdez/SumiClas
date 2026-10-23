using System.ComponentModel.DataAnnotations;
using MediatR;
using SumiClas.Api.Http;
using SumiClas.Assignments.Application;
using SumiClas.Shared.Application;

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
        RouteGroupBuilder assignments = app.MapGroup("/assignments").WithTags("Assignments");

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
            .ProducesValidationProblem();

        assignments.MapPost("/{assignmentId:guid}/comments", async (Guid assignmentId, AssignmentCommentAddRequest request, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new AssignmentCommentAdd.Command(assignmentId, request.Text), ct);

            return Results.StatusCode(StatusCodes.Status201Created);
        })
            .WithName("AssignmentCommentAdd")
            .Produces(StatusCodes.Status201Created);

        assignments.MapPut("/{assignmentId:guid}/done", async (Guid assignmentId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new AssignmentDoneMark.Command(assignmentId), ct);

            return Results.NoContent();
        })
            .WithName("AssignmentDoneMark")
            .Produces(StatusCodes.Status204NoContent);

        assignments.MapDelete("/{assignmentId:guid}/done", async (Guid assignmentId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new AssignmentDoneUnmark.Command(assignmentId), ct);

            return Results.NoContent();
        })
            .WithName("AssignmentDoneUnmark")
            .Produces(StatusCodes.Status204NoContent);

        assignments.MapGet("/", async (Guid classId, int? limit, string? cursor, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new AssignmentFinderByClass.Query(classId, limit, cursor), ct)))
            .WithName("AssignmentFinderByClass")
            .Produces<PagedResponse<AssignmentResponse>>(StatusCodes.Status200OK);
    }
}
