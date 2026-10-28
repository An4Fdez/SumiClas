using MediatR;
using SumiClas.CalendarEvents.Domain;
using SumiClas.Classes.Application;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.CalendarEvents.Application;

public static class CalendarEventUpdate
{
    /// <summary>
    /// The author changes the type, the title, the date, the time, the place and the cost. The scope, the class and
    /// the date of publication stay. A birthday is never changed.
    /// </summary>
    public sealed record Command(
        Guid Id,
        string Type,
        string Title,
        DateOnly Date,
        TimeOnly? Time,
        string? Location,
        string? Cost) : IRequest, ICommand;

    public sealed class Update(ICalendarEventRepository repository, IQueryBus queryBus, IClock clock, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            Guid current = userProvider.GetCurrentUserId();

            CalendarEvent calendarEvent = await repository.GetByIdAsync(command.Id, ct)
                ?? throw new CalendarEventExceptions.NotFound();

            if (calendarEvent.AuthorId != current)
            {
                throw new CalendarEventExceptions.ChangeForbidden();
            }

            if (!Enum.TryParse<EventType>(command.Type, ignoreCase: true, out EventType type) || !Enum.IsDefined(type))
            {
                throw new CalendarEventExceptions.InvalidType();
            }

            CalendarEvent.ValidateContent(type, command.Title);

            if (calendarEvent.ClassId is { } classId)
            {
                ClassResponse schoolClass = await queryBus.Send(new ClassFinderById.Query(classId), ct);

                await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.SchoolYearId), ct);
            }

            calendarEvent.Update(type, command.Title, command.Date, command.Time, command.Location, command.Cost, clock);
        }
    }

    public sealed class Handler(Update update) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => update.ExecuteAsync(command, ct);
    }
}
