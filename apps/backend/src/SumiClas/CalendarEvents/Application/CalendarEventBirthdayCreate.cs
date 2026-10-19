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
        : IRequest<Result>, ICommand;

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

            var creation = CalendarEvent.CreateBirthday(
                command.Id,
                command.StudentId,
                command.Title,
                command.Month,
                command.Day,
                authorId,
                clock);
            if (creation.IsFailure)
            {
                return Result.Failure(creation.Error!);
            }

            var author = await queryBus.Send(new UserFinderById.Query(authorId), ct);
            if (author.IsFailure)
            {
                return Result.Failure(author.Error!);
            }

            var guardian = await queryBus.Send(new StudentGuardianEnsure.Query(command.StudentId, authorId), ct);
            if (guardian.IsFailure)
            {
                return guardian;
            }

            await repository.AddAsync(creation.Value, ct);

            return Result.Success();
        }
    }

    public sealed class Handler(Create create) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => create.ExecuteAsync(command, ct);
    }
}
