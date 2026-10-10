using System.ComponentModel.DataAnnotations;
using MediatR;
using SumiClas.Api.Http;
using SumiClas.LostItems.Application;
using SumiClas.Shared.Application;

namespace SumiClas.Api.Endpoints;

public sealed record LostItemCreateRequest(
    Guid Id,
    string Scope,
    Guid? ClassId,
    string Status,
    string Description,
    [StringLength(2048)] string? PhotoUrl,
    bool PhotoPrivacyConfirmed);

public sealed class LostItemsEndpoints : IEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        var lostItems = app.MapGroup("/lost-items").WithTags("LostItems");

        lostItems.MapPost("/", async (LostItemCreateRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(
                new LostItemCreate.Command(
                    request.Id,
                    request.Scope,
                    request.ClassId,
                    request.Status,
                    request.Description,
                    request.PhotoUrl,
                    request.PhotoPrivacyConfirmed),
                ct);

            return result.ToCreated();
        })
            .WithName("LostItemCreate")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        lostItems.MapPut("/{lostItemId:guid}/recovered", async (Guid lostItemId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new LostItemRecoveredMark.Command(lostItemId), ct);

            return result.ToNoContent();
        })
            .WithName("LostItemRecoveredMark")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        lostItems.MapDelete("/{lostItemId:guid}/recovered", async (Guid lostItemId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new LostItemNotRecoveredMark.Command(lostItemId), ct);

            return result.ToNoContent();
        })
            .WithName("LostItemNotRecoveredMark")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        lostItems.MapGet("/", async (Guid classId, int? limit, string? cursor, ISender sender, CancellationToken ct) =>
            (await sender.Send(new LostItemFinderByClass.Query(classId, limit, cursor), ct)).ToOk())
            .WithName("LostItemFinderByClass")
            .Produces<PagedResponse<LostItemResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
