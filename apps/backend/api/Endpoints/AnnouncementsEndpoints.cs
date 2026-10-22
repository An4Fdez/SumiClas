using System.ComponentModel.DataAnnotations;
using MediatR;
using SumiClas.Announcements.Application;
using SumiClas.Announcements.Domain;
using SumiClas.CalendarEvents.Domain;
using SumiClas.Api.Http;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.Api.Endpoints;

public sealed record AnnouncementCreateRequest(
    Guid Id,
    string Scope,
    Guid? ClassId,
    [StringLength(200)] string Title,
    string Body,
    string Priority,
    Guid? RelatedEventId);

public sealed class AnnouncementsEndpoints : IEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        var announcements = app.MapGroup("/announcements").WithTags("Announcements");

        announcements.MapPost("/", async (AnnouncementCreateRequest request, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(
                new AnnouncementCreate.Command(
                    request.Id,
                    request.Scope,
                    request.ClassId,
                    request.Title,
                    request.Body,
                    request.Priority,
                    request.RelatedEventId),
                ct);

            return Results.StatusCode(StatusCodes.Status201Created);
        })
            .WithName("AnnouncementCreate")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Maps<AnnouncementExceptions.IdRequired>(StatusCodes.Status400BadRequest)
            .Maps<AnnouncementExceptions.InvalidScope>(StatusCodes.Status400BadRequest)
            .Maps<AnnouncementExceptions.InvalidPriority>(StatusCodes.Status400BadRequest)
            .Maps<AnnouncementExceptions.ClassRequiredForClassScope>(StatusCodes.Status400BadRequest)
            .Maps<AnnouncementExceptions.ClassRequiredForSchoolScope>(StatusCodes.Status400BadRequest)
            .Maps<AnnouncementExceptions.TitleRequired>(StatusCodes.Status400BadRequest)
            .Maps<AnnouncementExceptions.BodyRequired>(StatusCodes.Status400BadRequest)
            .Maps<AnnouncementExceptions.AuthorRequired>(StatusCodes.Status400BadRequest)
            .Maps<CalendarEventExceptions.NotFound>(StatusCodes.Status404NotFound)
            .RequiresSession()
            .RequiresUser()
            .RequiresClass()
            .RequiresClassRepresentative()
            .RequiresActiveSchoolYear()
            .ConflictsOnSave();

        announcements.MapPut("/{announcementId:guid}/read", async (Guid announcementId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new AnnouncementReadMark.Command(announcementId), ct);

            return Results.NoContent();
        })
            .WithName("AnnouncementReadMark")
            .Produces(StatusCodes.Status204NoContent)
            .Maps<AnnouncementExceptions.NotFound>(StatusCodes.Status404NotFound)
            .Maps<AnnouncementExceptions.ReadReceiptUserRequired>(StatusCodes.Status400BadRequest)
            .RequiresSession()
            .RequiresUser()
            .RequiresClassMembership()
            .ConflictsOnSave();

        announcements.MapGet("/", async (Guid classId, int? limit, string? cursor, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new AnnouncementFinderByClass.Query(classId, limit, cursor), ct)))
            .WithName("AnnouncementFinderByClass")
            .Produces<PagedResponse<AnnouncementResponse>>(StatusCodes.Status200OK)
            .Maps<PaginationExceptions.InvalidLimit>(StatusCodes.Status400BadRequest)
            .Maps<PaginationExceptions.InvalidCursor>(StatusCodes.Status400BadRequest)
            .RequiresSession()
            .RequiresClass()
            .RequiresClassMembership()
            .Maps<SchoolYearExceptions.NotFound>(StatusCodes.Status404NotFound);
    }
}
