using System.ComponentModel.DataAnnotations;
using MediatR;
using SumiClas.Api.Http;
using SumiClas.ClassExpenses.Application;

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
        var classExpenses = app.MapGroup("/class-expenses");

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
        });

        classExpenses.MapPut("/{classExpenseId:guid}/participants/{guardianId:guid}/paid", async (Guid classExpenseId, Guid guardianId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ClassExpensePaidMark.Command(classExpenseId, guardianId), ct);

            return result.ToNoContent();
        });

        classExpenses.MapDelete("/{classExpenseId:guid}/participants/{guardianId:guid}/paid", async (Guid classExpenseId, Guid guardianId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ClassExpenseUnpaidMark.Command(classExpenseId, guardianId), ct);

            return result.ToNoContent();
        });
    }
}
