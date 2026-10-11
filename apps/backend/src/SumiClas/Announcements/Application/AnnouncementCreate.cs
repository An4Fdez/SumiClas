using MediatR;
using SumiClas.Announcements.Domain;
using SumiClas.CalendarEvents.Application;
using SumiClas.Classes.Application;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.Announcements.Application;

public static class AnnouncementCreate
{
    public sealed record Command(
        Guid Id,
        string Scope,
        Guid? ClassId,
        string Title,
        string Body,
        string Priority,
        Guid? RelatedEventId) : IRequest<Result>, ICommand;

    public sealed class Create(IAnnouncementRepository announcements, IQueryBus queryBus, IClock clock, IUserProvider userProvider) : IUseCase
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var current = userProvider.GetCurrentUserId();
            if (current.IsFailure)
            {
                return Result.Failure(current.Error!);
            }

            var authorId = current.Value;

            if (!Enum.TryParse<AnnouncementScope>(command.Scope, ignoreCase: true, out var scope) || !Enum.IsDefined(scope))
            {
                return Result.Failure(AnnouncementErrors.InvalidScope);
            }
            if (!Enum.TryParse<Priority>(command.Priority, ignoreCase: true, out var priority) || !Enum.IsDefined(priority))
            {
                return Result.Failure(AnnouncementErrors.InvalidPriority);
            }

            var creation = Announcement.Create(
                command.Id,
                scope,
                command.ClassId,
                command.Title,
                command.Body,
                priority,
                authorId,
                command.RelatedEventId,
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

            if (command.RelatedEventId is { } relatedEventId)
            {
                var relatedEvent = await queryBus.Send(new CalendarEventFinderById.Query(relatedEventId), ct);
                if (relatedEvent.IsFailure)
                {
                    return Result.Failure(relatedEvent.Error!);
                }
            }

            if (scope == AnnouncementScope.Class)
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

            await announcements.AddAsync(creation.Value, ct);

            return Result.Success();
        }
    }

    public sealed class Handler(Create create) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => create.ExecuteAsync(command, ct);
    }
}
