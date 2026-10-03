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
    DateOnly DueDate,
    Guid AuthorId);

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
                    request.DueDate,
                    request.AuthorId),
                ct);

            return result.IsSuccess
                ? Results.StatusCode(StatusCodes.Status201Created)
                : result.ToProblem();
        });
    }
}
