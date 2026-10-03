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
    public sealed record Command(
        Guid Id,
        string Scope,
        Guid? ClassId,
        string Type,
        string Title,
        DateOnly Date,
        TimeOnly? Time,
        string? Location,
        string? Cost,
        Guid AuthorId) : IRequest<Result>, ICommand;

    public sealed class UseCase(ICalendarEventRepository calendarEvents, IQueryBus queryBus, IClock clock)
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            if (!Enum.TryParse<EventScope>(command.Scope, ignoreCase: true, out var scope) || !Enum.IsDefined(scope))
            {
                return Result.Failure(CalendarEventErrors.InvalidScope);
            }
            if (!Enum.TryParse<EventType>(command.Type, ignoreCase: true, out var type) || !Enum.IsDefined(type))
            {
                return Result.Failure(CalendarEventErrors.InvalidType);
            }

            var creation = CalendarEvent.Create(
                command.Id,
                scope,
                command.ClassId,
                type,
                command.Title,
                command.Date,
                command.Time,
                command.Location,
                command.Cost,
                command.AuthorId,
                clock);
            if (creation.IsFailure)
            {
                return Result.Failure(creation.Error!);
            }

            var author = await queryBus.Send(new UserFinderById.Query(command.AuthorId), ct);
            if (author.IsFailure)
            {
                return Result.Failure(author.Error!);
            }

            if (scope == EventScope.Class)
            {
                var schoolClass = await queryBus.Send(new ClassFinderById.Query(command.ClassId!.Value), ct);
                if (schoolClass.IsFailure)
                {
                    return Result.Failure(schoolClass.Error!);
                }

                var schoolYear = await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.Value.SchoolYearId), ct);
                if (schoolYear.IsFailure)
                {
                    return schoolYear;
                }
            }

            await calendarEvents.AddAsync(creation.Value, ct);

            return Result.Success();
        }
    }

    public sealed class Handler(UseCase useCase) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => useCase.ExecuteAsync(command, ct);
    }
}
