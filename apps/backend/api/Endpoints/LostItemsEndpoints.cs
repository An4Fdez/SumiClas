using System.ComponentModel.DataAnnotations;
using MediatR;
using SumiClas.Api.Http;
using SumiClas.LostItems.Application;
using SumiClas.LostItems.Domain;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

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
            .ProducesValidationProblem()
            .Maps<LostItemExceptions.IdRequired>(StatusCodes.Status400BadRequest)
            .Maps<LostItemExceptions.InvalidScope>(StatusCodes.Status400BadRequest)
            .Maps<LostItemExceptions.InvalidStatus>(StatusCodes.Status400BadRequest)
            .Maps<LostItemExceptions.ClassRequiredForClassScope>(StatusCodes.Status400BadRequest)
            .Maps<LostItemExceptions.ClassRequiredForSchoolScope>(StatusCodes.Status400BadRequest)
            .Maps<LostItemExceptions.DescriptionRequired>(StatusCodes.Status400BadRequest)
            .Maps<LostItemExceptions.PhotoPrivacyConfirmationRequired>(StatusCodes.Status400BadRequest)
            .Maps<LostItemExceptions.AuthorRequired>(StatusCodes.Status400BadRequest)
            .RequiresSession()
            .RequiresUser()
            .RequiresClass()
            .RequiresClassMembership()
            .RequiresActiveSchoolYear()
            .ConflictsOnSave();

        lostItems.MapPut("/{lostItemId:guid}/recovered", async (Guid lostItemId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new LostItemRecoveredMark.Command(lostItemId), ct);

            return Results.NoContent();
        })
            .WithName("LostItemRecoveredMark")
            .Produces(StatusCodes.Status204NoContent)
            .Maps<LostItemExceptions.NotFound>(StatusCodes.Status404NotFound)
            .Maps<LostItemExceptions.MarkForbidden>(StatusCodes.Status403Forbidden)
            .RequiresSession()
            .RequiresClass()
            .RequiresActiveSchoolYear()
            .ConflictsOnSave();

        lostItems.MapDelete("/{lostItemId:guid}/recovered", async (Guid lostItemId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new LostItemNotRecoveredMark.Command(lostItemId), ct);

            return Results.NoContent();
        })
            .WithName("LostItemNotRecoveredMark")
            .Produces(StatusCodes.Status204NoContent)
            .Maps<LostItemExceptions.NotFound>(StatusCodes.Status404NotFound)
            .Maps<LostItemExceptions.MarkForbidden>(StatusCodes.Status403Forbidden)
            .RequiresSession()
            .RequiresClass()
            .RequiresActiveSchoolYear()
            .ConflictsOnSave();

        lostItems.MapGet("/", async (Guid classId, int? limit, string? cursor, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new LostItemFinderByClass.Query(classId, limit, cursor), ct)))
            .WithName("LostItemFinderByClass")
            .Produces<PagedResponse<LostItemResponse>>(StatusCodes.Status200OK)
            .Maps<PaginationExceptions.InvalidLimit>(StatusCodes.Status400BadRequest)
            .Maps<PaginationExceptions.InvalidCursor>(StatusCodes.Status400BadRequest)
            .RequiresSession()
            .RequiresClass()
            .RequiresClassMembership()
            .Maps<SchoolYearExceptions.NotFound>(StatusCodes.Status404NotFound);
    }
}
