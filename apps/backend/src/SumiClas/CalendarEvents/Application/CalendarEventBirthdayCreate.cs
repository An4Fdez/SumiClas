using MediatR;
using SumiClas.CalendarEvents.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Students.Application;
using SumiClas.Users.Application;

namespace SumiClas.CalendarEvents.Application;

public static class CalendarEventBirthdayCreate
{
    /// <summary>Month and day only: a birth year is never stored (privacy rule for minors).</summary>
    public sealed record Command(Guid Id, Guid StudentId, string Title, int Month, int Day, Guid AuthorId)
        : IRequest<Result>, ICommand;

    public sealed class UseCase(ICalendarEventRepository calendarEvents, IQueryBus queryBus, IClock clock)
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var creation = CalendarEvent.CreateBirthday(
                command.Id,
                command.StudentId,
                command.Title,
                command.Month,
                command.Day,
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

            var student = await queryBus.Send(new StudentFinderById.Query(command.StudentId), ct);
            if (student.IsFailure)
            {
                return Result.Failure(student.Error!);
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
