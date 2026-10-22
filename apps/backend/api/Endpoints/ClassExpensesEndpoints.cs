using System.ComponentModel.DataAnnotations;
using MediatR;
using SumiClas.Api.Http;
using SumiClas.ClassExpenses.Application;
using SumiClas.Shared.Application;

namespace SumiClas.Api.Endpoints;

public sealed record ClassExpenseCreateRequest(
    Guid Id,
    Guid ClassId,
    [StringLength(200)] string Item,
    [Range(0.01, 99999999.99)] decimal TotalAmount,
    [Required] IReadOnlyCollection<Guid> ParticipantIds,
    Guid? RelatedEventId);

public sealed class ClassExpensesEndpoints : IEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        var classExpenses = app.MapGroup("/class-expenses").WithTags("ClassExpenses");

        classExpenses.MapPost("/", async (ClassExpenseCreateRequest request, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(
                new ClassExpenseCreate.Command(
                    request.Id,
                    request.ClassId,
                    request.Item,
                    request.TotalAmount,
                    request.ParticipantIds,
                    request.RelatedEventId),
                ct);

            return Results.StatusCode(StatusCodes.Status201Created);
        })
            .WithName("ClassExpenseCreate")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        classExpenses.MapPut("/{classExpenseId:guid}/participants/{guardianId:guid}/paid", async (Guid classExpenseId, Guid guardianId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new ClassExpensePaidMark.Command(classExpenseId, guardianId), ct);

            return Results.NoContent();
        })
            .WithName("ClassExpensePaidMark")
            .Produces(StatusCodes.Status204NoContent);

        classExpenses.MapDelete("/{classExpenseId:guid}/participants/{guardianId:guid}/paid", async (Guid classExpenseId, Guid guardianId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new ClassExpenseUnpaidMark.Command(classExpenseId, guardianId), ct);

            return Results.NoContent();
        })
            .WithName("ClassExpenseUnpaidMark")
            .Produces(StatusCodes.Status204NoContent);

        classExpenses.MapGet("/", async (Guid classId, int? limit, string? cursor, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new ClassExpenseFinderByClass.Query(classId, limit, cursor), ct)))
            .WithName("ClassExpenseFinderByClass")
            .Produces<PagedResponse<ClassExpenseResponse>>(StatusCodes.Status200OK);
    }
}
