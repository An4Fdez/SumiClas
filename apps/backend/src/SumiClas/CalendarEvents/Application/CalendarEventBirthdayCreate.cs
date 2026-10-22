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
    public sealed record Command(Guid Id, Guid StudentId, string Title, int Month, int Day)
        : IRequest, ICommand;

    public sealed class Create(ICalendarEventRepository repository, IQueryBus queryBus, IClock clock, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            Guid current = userProvider.GetCurrentUserId();

            Guid authorId = current;

            CalendarEvent birthday = CalendarEvent.CreateBirthday(
                command.Id,
                command.StudentId,
                command.Title,
                command.Month,
                command.Day,
                authorId,
                clock);

            await queryBus.Send(new UserFinderById.Query(authorId), ct);

            await queryBus.Send(new StudentGuardianEnsure.Query(command.StudentId, authorId), ct);

            await repository.AddAsync(birthday, ct);
        }
    }

    public sealed class Handler(Create create) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => create.ExecuteAsync(command, ct);
    }
}
