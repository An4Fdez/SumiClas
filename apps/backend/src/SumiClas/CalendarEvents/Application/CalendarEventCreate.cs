using MediatR;
using SumiClas.CalendarEvents.Domain;
using SumiClas.Classes.Application;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.CalendarEvents.Application;

public static class CalendarEventCreate
{
    /// <summary>
    /// ClassId is always the class the event is published from, and the author must be its delegate. For a
    /// school-wide event the class only gives the school the event belongs to; it is not stored.
    /// </summary>
    public sealed record Command(
        Guid Id,
        string Scope,
        Guid? ClassId,
        string Type,
        string Title,
        DateOnly Date,
        TimeOnly? Time,
        string? Location,
        string? Cost) : IRequest, ICommand;

    public sealed class Create(ICalendarEventRepository repository, IQueryBus queryBus, IClock clock, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            Guid current = userProvider.GetCurrentUserId();

            Guid authorId = current;

            if (!Enum.TryParse<EventScope>(command.Scope, ignoreCase: true, out EventScope scope) || !Enum.IsDefined(scope))
            {
                throw new CalendarEventExceptions.InvalidScope();
            }
            if (!Enum.TryParse<EventType>(command.Type, ignoreCase: true, out EventType type) || !Enum.IsDefined(type))
            {
                throw new CalendarEventExceptions.InvalidType();
            }

            // The school of a school-wide event is only known after looking up the class, so the aggregate is built
            // twice: once only to validate the input before any IO (the school is provisional and discarded), and
            // once with the real school.
            CalendarEvent calendarEvent = CalendarEvent.Create(
                command.Id,
                scope,
                command.ClassId,
                scope == EventScope.School ? Guid.NewGuid() : null,
                type,
                command.Title,
                command.Date,
                command.Time,
                command.Location,
                command.Cost,
                authorId,
                clock);

            if (command.ClassId is not { } classId || classId == Guid.Empty)
            {
                throw new CalendarEventExceptions.ClassRequiredForSchoolScope();
            }

            UserResponse author = await queryBus.Send(new UserFinderById.Query(authorId), ct);

            ClassResponse schoolClass = await queryBus.Send(new ClassFinderById.Query(classId), ct);

            await queryBus.Send(new UserClassRepresentativeEnsure.Query(authorId, classId), ct);

            await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.SchoolYearId), ct);

            if (scope == EventScope.School)
            {
                Guid school = await queryBus.Send(new SchoolIdFinderBySchoolYear.Query(schoolClass.SchoolYearId), ct);

                await queryBus.Send(new UserSchoolPublishEnsure.Query(authorId, school), ct);

                calendarEvent = CalendarEvent.Create(
                    command.Id,
                    scope,
                    command.ClassId,
                    school,
                    type,
                    command.Title,
                    command.Date,
                    command.Time,
                    command.Location,
                    command.Cost,
                    authorId,
                    clock);
            }

            await repository.AddAsync(calendarEvent, ct);
        }
    }

    public sealed class Handler(Create create) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => create.ExecuteAsync(command, ct);
    }
}
