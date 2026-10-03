using System.ComponentModel.DataAnnotations;
using MediatR;
using SumiClas.Api.Http;
using SumiClas.Assignments.Application;

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
        var assignments = app.MapGroup("/assignments");

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
        });

        assignments.MapPost("/{assignmentId:guid}/comments", async (Guid assignmentId, AssignmentCommentAddRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new AssignmentCommentAdd.Command(assignmentId, request.Text), ct);

            return result.ToCreated();
        });

        assignments.MapGet("/", async (Guid classId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new AssignmentFinderByClass.Query(classId), ct)).ToOk());
    }
}
