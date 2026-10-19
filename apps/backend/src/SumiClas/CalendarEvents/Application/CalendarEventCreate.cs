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
        string? Cost) : IRequest<Result>, ICommand;

    public sealed class Create(ICalendarEventRepository repository, IQueryBus queryBus, IClock clock, IUserProvider userProvider) : IUseCase
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var current = userProvider.GetCurrentUserId();
            if (current.IsFailure)
            {
                return Result.Failure(current.Error!);
            }

            var authorId = current.Value;

            if (!Enum.TryParse<EventScope>(command.Scope, ignoreCase: true, out var scope) || !Enum.IsDefined(scope))
            {
                return Result.Failure(CalendarEventErrors.InvalidScope);
            }
            if (!Enum.TryParse<EventType>(command.Type, ignoreCase: true, out var type) || !Enum.IsDefined(type))
            {
                return Result.Failure(CalendarEventErrors.InvalidType);
            }

            // The school of a school-wide event is only known after looking up the class, so the aggregate is built
            // twice: once only to validate the input before any IO (the school is provisional and discarded), and
            // once with the real school.
            var validation = CalendarEvent.Create(
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
            if (validation.IsFailure)
            {
                return Result.Failure(validation.Error!);
            }

            if (command.ClassId is not { } classId || classId == Guid.Empty)
            {
                return Result.Failure(CalendarEventErrors.ClassRequiredForSchoolScope);
            }

            var author = await queryBus.Send(new UserFinderById.Query(authorId), ct);
            if (author.IsFailure)
            {
                return Result.Failure(author.Error!);
            }

            var schoolClass = await queryBus.Send(new ClassFinderById.Query(classId), ct);
            if (schoolClass.IsFailure)
            {
                return Result.Failure(schoolClass.Error!);
            }

            var representative = await queryBus.Send(new UserClassRepresentativeEnsure.Query(authorId, classId), ct);
            if (representative.IsFailure)
            {
                return representative;
            }

            var schoolYear = await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.Value.SchoolYearId), ct);
            if (schoolYear.IsFailure)
            {
                return schoolYear;
            }

            var calendarEvent = validation.Value;
            if (scope == EventScope.School)
            {
                var school = await queryBus.Send(new SchoolIdFinderBySchoolYear.Query(schoolClass.Value.SchoolYearId), ct);
                if (school.IsFailure)
                {
                    return Result.Failure(school.Error!);
                }

                var creation = CalendarEvent.Create(
                    command.Id,
                    scope,
                    command.ClassId,
                    school.Value,
                    type,
                    command.Title,
                    command.Date,
                    command.Time,
                    command.Location,
                    command.Cost,
                    authorId,
                    clock);
                if (creation.IsFailure)
                {
                    return Result.Failure(creation.Error!);
                }

                calendarEvent = creation.Value;
            }

            await repository.AddAsync(calendarEvent, ct);

            return Result.Success();
        }
    }

    public sealed class Handler(Create create) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => create.ExecuteAsync(command, ct);
    }
}
