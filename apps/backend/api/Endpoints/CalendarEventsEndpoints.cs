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
    [StringLength(100)] string? Cost,
    Guid AuthorId);

public sealed record CalendarEventBirthdayCreateRequest(
    Guid Id,
    Guid StudentId,
    [StringLength(200)] string Title,
    int Month,
    int Day,
    Guid AuthorId);

public sealed class CalendarEventsEndpoints : IEndpoints
{
    public void Map(IEndpointRouteBuilder app)
    {
        var calendarEvents = app.MapGroup("/calendar-events");

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
                    request.Cost,
                    request.AuthorId),
                ct);

            return result.IsSuccess
                ? Results.StatusCode(StatusCodes.Status201Created)
                : result.ToProblem();
        });

        calendarEvents.MapPost("/birthdays", async (CalendarEventBirthdayCreateRequest request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(
                new CalendarEventBirthdayCreate.Command(
                    request.Id,
                    request.StudentId,
                    request.Title,
                    request.Month,
                    request.Day,
                    request.AuthorId),
                ct);

            return result.IsSuccess
                ? Results.StatusCode(StatusCodes.Status201Created)
                : result.ToProblem();
        });
    }
}
