using System.ComponentModel.DataAnnotations;
using MediatR;
using SumiClas.Announcements.Application;
using SumiClas.Api.Http;
using SumiClas.Shared.Application;

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
        RouteGroupBuilder announcements = app.MapGroup("/announcements").WithTags("Announcements");

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
            .ProducesValidationProblem();

        announcements.MapPut("/{announcementId:guid}/read", async (Guid announcementId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new AnnouncementReadMark.Command(announcementId), ct);

            return Results.NoContent();
        })
            .WithName("AnnouncementReadMark")
            .Produces(StatusCodes.Status204NoContent);

        announcements.MapGet("/", async (Guid classId, int? limit, string? cursor, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new AnnouncementFinderByClass.Query(classId, limit, cursor), ct)))
            .WithName("AnnouncementFinderByClass")
            .Produces<PagedResponse<AnnouncementResponse>>(StatusCodes.Status200OK);
    }
}
