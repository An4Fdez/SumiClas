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
        var assignments = app.MapGroup("/assignments").WithTags("Assignments");

        assignments.MapPost("/", async (AssignmentCreateRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(
                new AssignmentCreate.Command(
                    request.Id,
                    request.ClassId,
                    request.SubjectId,
                    request.Title,
                    request.Description,
                    request.DueDate),
                ct);

            return result.ToCreated();
        })
            .WithName("AssignmentCreate")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        assignments.MapPost("/{assignmentId:guid}/comments", async (Guid assignmentId, AssignmentCommentAddRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new AssignmentCommentAdd.Command(assignmentId, request.Text), ct);

            return result.ToCreated();
        })
            .WithName("AssignmentCommentAdd")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        assignments.MapGet("/", async (Guid classId, int? limit, string? cursor, ISender sender, CancellationToken ct) =>
            (await sender.Send(new AssignmentFinderByClass.Query(classId, limit, cursor), ct)).ToOk())
            .WithName("AssignmentFinderByClass")
            .Produces<PagedResponse<AssignmentResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
