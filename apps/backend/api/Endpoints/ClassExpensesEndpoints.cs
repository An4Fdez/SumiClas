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
            var result = await sender.Send(
                new ClassExpenseCreate.Command(
                    request.Id,
                    request.ClassId,
                    request.Item,
                    request.TotalAmount,
                    request.ParticipantIds,
                    request.RelatedEventId),
                ct);

            return result.ToCreated();
        })
            .WithName("ClassExpenseCreate")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        classExpenses.MapPut("/{classExpenseId:guid}/participants/{guardianId:guid}/paid", async (Guid classExpenseId, Guid guardianId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ClassExpensePaidMark.Command(classExpenseId, guardianId), ct);

            return result.ToNoContent();
        })
            .WithName("ClassExpensePaidMark")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        classExpenses.MapDelete("/{classExpenseId:guid}/participants/{guardianId:guid}/paid", async (Guid classExpenseId, Guid guardianId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ClassExpenseUnpaidMark.Command(classExpenseId, guardianId), ct);

            return result.ToNoContent();
        })
            .WithName("ClassExpenseUnpaidMark")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        classExpenses.MapGet("/", async (Guid classId, int? limit, string? cursor, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ClassExpenseFinderByClass.Query(classId, limit, cursor), ct)).ToOk())
            .WithName("ClassExpenseFinderByClass")
            .Produces<PagedResponse<ClassExpenseResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
