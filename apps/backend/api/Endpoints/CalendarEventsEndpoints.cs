using System.ComponentModel.DataAnnotations;
using MediatR;
using SumiClas.Api.Http;
using SumiClas.CalendarEvents.Application;

namespace SumiClas.Api.Endpoints;

public sealed record CalendarEventCreateRequest(
    Guid Id,
    string Scope,
    Guid? ClassId,
    string Type,
    [StringLength(200)] string Title,
    DateOnly Date,
    TimeOnly? Time,
    [StringLength(200)] string? Location,
    [StringLength(100)] string? Cost);

public sealed record CalendarEventBirthdayCreateRequest(
    Guid Id,
    Guid StudentId,
    [StringLength(200)] string Title,
    int Month,
    int Day);

public sealed class CalendarEventsEndpoints : IEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        var calendarEvents = app.MapGroup("/calendar-events").WithTags("CalendarEvents");

        calendarEvents.MapPost("/", async (CalendarEventCreateRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(
                new CalendarEventCreate.Command(
                    request.Id,
                    request.Scope,
                    request.ClassId,
                    request.Type,
                    request.Title,
                    request.Date,
                    request.Time,
                    request.Location,
                    request.Cost),
                ct);

            return result.IsSuccess
                ? Results.StatusCode(StatusCodes.Status201Created)
                : result.ToProblem();
        })
            .WithName("CalendarEventCreate")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        calendarEvents.MapPost("/birthdays", async (CalendarEventBirthdayCreateRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(
                new CalendarEventBirthdayCreate.Command(
                    request.Id,
                    request.StudentId,
                    request.Title,
                    request.Month,
                    request.Day),
                ct);

            return result.IsSuccess
                ? Results.StatusCode(StatusCodes.Status201Created)
                : result.ToProblem();
        })
            .WithName("CalendarEventBirthdayCreate")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        calendarEvents.MapGet("/", async (Guid classId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new CalendarEventFinderByClass.Query(classId), ct)).ToOk())
            .WithName("CalendarEventFinderByClass")
            .Produces<IReadOnlyCollection<CalendarEventResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
