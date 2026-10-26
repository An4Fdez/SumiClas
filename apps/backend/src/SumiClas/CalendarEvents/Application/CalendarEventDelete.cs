using MediatR;
using SumiClas.CalendarEvents.Domain;
using SumiClas.Classes.Application;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;

namespace SumiClas.CalendarEvents.Application;

public static class CalendarEventDelete
{
    /// <summary>
    /// The author deletes the event for good. A birthday can be deleted too. The announcements and expenses that
    /// pointed at it just lose their related event.
    /// </summary>
    public sealed record Command(Guid Id) : IRequest, ICommand;

    public sealed class Delete(ICalendarEventRepository repository, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
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

            if (calendarEvent.ClassId is { } classId)
            {
                ClassResponse schoolClass = await queryBus.Send(new ClassFinderById.Query(classId), ct);

                await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.SchoolYearId), ct);
            }

            repository.Remove(calendarEvent);
        }
    }

    public sealed class Handler(Delete delete) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => delete.ExecuteAsync(command, ct);
    }
}
