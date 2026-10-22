using System.ComponentModel.DataAnnotations;
using MediatR;
using SumiClas.Api.Http;
using SumiClas.CalendarEvents.Application;
using SumiClas.CalendarEvents.Domain;

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
            await sender.Send(
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

            return Results.StatusCode(StatusCodes.Status201Created);
        })
            .WithName("CalendarEventCreate")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Maps<CalendarEventExceptions.IdRequired>(StatusCodes.Status400BadRequest)
            .Maps<CalendarEventExceptions.InvalidScope>(StatusCodes.Status400BadRequest)
            .Maps<CalendarEventExceptions.InvalidType>(StatusCodes.Status400BadRequest)
            .Maps<CalendarEventExceptions.UseCreateBirthdayForBirthdays>(StatusCodes.Status400BadRequest)
            .Maps<CalendarEventExceptions.ClassRequiredForClassScope>(StatusCodes.Status400BadRequest)
            .Maps<CalendarEventExceptions.ClassRequiredForSchoolScope>(StatusCodes.Status400BadRequest)
            .Maps<CalendarEventExceptions.TitleRequired>(StatusCodes.Status400BadRequest)
            .Maps<CalendarEventExceptions.AuthorRequired>(StatusCodes.Status400BadRequest)
            .RequiresSession()
            .RequiresUser()
            .RequiresClass()
            .RequiresClassRepresentative()
            .RequiresActiveSchoolYear()
            .ConflictsOnSave();

        calendarEvents.MapPost("/birthdays", async (CalendarEventBirthdayCreateRequest request, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(
                new CalendarEventBirthdayCreate.Command(
                    request.Id,
                    request.StudentId,
                    request.Title,
                    request.Month,
                    request.Day),
                ct);

            return Results.StatusCode(StatusCodes.Status201Created);
        })
            .WithName("CalendarEventBirthdayCreate")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Maps<CalendarEventExceptions.IdRequired>(StatusCodes.Status400BadRequest)
            .Maps<CalendarEventExceptions.StudentRequiredForBirthday>(StatusCodes.Status400BadRequest)
            .Maps<CalendarEventExceptions.TitleRequired>(StatusCodes.Status400BadRequest)
            .Maps<CalendarEventExceptions.AuthorRequired>(StatusCodes.Status400BadRequest)
            .Maps<CalendarEventExceptions.InvalidMonth>(StatusCodes.Status400BadRequest)
            .Maps<CalendarEventExceptions.InvalidDay>(StatusCodes.Status400BadRequest)
            .RequiresSession()
            .RequiresUser()
            .RequiresStudentGuardian()
            .ConflictsOnSave();

        calendarEvents.MapGet("/", async (Guid classId, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new CalendarEventFinderByClass.Query(classId), ct)))
            .WithName("CalendarEventFinderByClass")
            .Produces<IReadOnlyCollection<CalendarEventResponse>>(StatusCodes.Status200OK)
            .RequiresSession()
            .RequiresClass()
            .RequiresClassMembership();
    }
}
