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

public sealed record LostItemUpdateRequest(string Description);

public sealed record LostItemClaimCreateRequest(
    Guid Id,
    [StringLength(LostItemClaimLimits.MessageMaxLength)] string Message);

public sealed record LostItemClaimAcceptRequest(
    [StringLength(LostItemClaimLimits.ResponseMessageMaxLength)] string? ResponseMessage);

public sealed class LostItemsEndpoints : IEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder lostItems = app.MapGroup("/lost-items").WithTags("LostItems");

        lostItems.MapPost("/", async (LostItemCreateRequest request, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(
                new LostItemCreate.Command(
                    request.Id,
                    request.Scope,
                    request.ClassId,
                    request.Status,
                    request.Description,
                    request.PhotoUrl,
                    request.PhotoPrivacyConfirmed),
                ct);

            return Results.StatusCode(StatusCodes.Status201Created);
        })
            .WithName("LostItemCreate")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        lostItems.MapPut("/{lostItemId:guid}", async (Guid lostItemId, LostItemUpdateRequest request, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new LostItemUpdate.Command(lostItemId, request.Description), ct);

            return Results.NoContent();
        })
            .WithName("LostItemUpdate")
            .Produces(StatusCodes.Status204NoContent);

        lostItems.MapDelete("/{lostItemId:guid}", async (Guid lostItemId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new LostItemDelete.Command(lostItemId), ct);

            return Results.NoContent();
        })
            .WithName("LostItemDelete")
            .Produces(StatusCodes.Status204NoContent);

        lostItems.MapPut("/{lostItemId:guid}/recovered", async (Guid lostItemId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new LostItemRecoveredMark.Command(lostItemId), ct);

            return Results.NoContent();
        })
            .WithName("LostItemRecoveredMark")
            .Produces(StatusCodes.Status204NoContent);

        lostItems.MapDelete("/{lostItemId:guid}/recovered", async (Guid lostItemId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new LostItemNotRecoveredMark.Command(lostItemId), ct);

            return Results.NoContent();
        })
            .WithName("LostItemNotRecoveredMark")
            .Produces(StatusCodes.Status204NoContent);

        lostItems.MapGet("/", async (Guid classId, int? limit, string? cursor, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new LostItemFinderByClass.Query(classId, limit, cursor), ct)))
            .WithName("LostItemFinderByClass")
            .Produces<PagedResponse<LostItemResponse>>(StatusCodes.Status200OK);

        lostItems.MapPost("/{lostItemId:guid}/claims", async (Guid lostItemId, LostItemClaimCreateRequest request, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new LostItemClaimCreate.Command(request.Id, lostItemId, request.Message), ct);

            return Results.StatusCode(StatusCodes.Status201Created);
        })
            .WithName("LostItemClaimCreate")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        lostItems.MapGet("/{lostItemId:guid}/claims", async (Guid lostItemId, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new LostItemClaimFinderByItem.Query(lostItemId), ct)))
            .WithName("LostItemClaimFinderByItem")
            .Produces<IReadOnlyCollection<LostItemClaimResponse>>(StatusCodes.Status200OK);

        lostItems.MapPut("/{lostItemId:guid}/claims/{claimId:guid}/accept", async (Guid lostItemId, Guid claimId, LostItemClaimAcceptRequest? request, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new LostItemClaimAccept.Command(lostItemId, claimId, request?.ResponseMessage), ct);

            return Results.NoContent();
        })
            .WithName("LostItemClaimAccept")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        lostItems.MapPut("/{lostItemId:guid}/claims/{claimId:guid}/reject", async (Guid lostItemId, Guid claimId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new LostItemClaimReject.Command(lostItemId, claimId), ct);

            return Results.NoContent();
        })
            .WithName("LostItemClaimReject")
            .Produces(StatusCodes.Status204NoContent);
    }
}
