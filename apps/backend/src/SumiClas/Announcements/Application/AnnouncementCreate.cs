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
    /// <summary>
    /// ClassId is always the class the announcement is published from, and the author must be its delegate. For a
    /// school-wide announcement the class only gives the school the announcement belongs to; it is not stored.
    /// </summary>
    public sealed record Command(
        Guid Id,
        string Scope,
        Guid? ClassId,
        string Title,
        string Body,
        string Priority,
        Guid? RelatedEventId) : IRequest<Result>, ICommand;

    public sealed class Create(IAnnouncementRepository repository, IQueryBus queryBus, IClock clock, IUserProvider userProvider) : IUseCase
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

            // The school of a school-wide announcement is only known after looking up the class, so the aggregate is
            // built twice: once only to validate the input before any IO (the school is provisional and discarded),
            // and once with the real school.
            var validation = Announcement.Create(
                command.Id,
                scope,
                command.ClassId,
                scope == AnnouncementScope.School ? Guid.NewGuid() : null,
                command.Title,
                command.Body,
                priority,
                authorId,
                command.RelatedEventId,
                clock);
            if (validation.IsFailure)
            {
                return Result.Failure(validation.Error!);
            }

            if (command.ClassId is not { } classId || classId == Guid.Empty)
            {
                return Result.Failure(AnnouncementErrors.ClassRequiredForSchoolScope);
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

            var schoolClass = await queryBus.Send(new ClassFinderById.Query(classId), ct);

            var representative = await queryBus.Send(new UserClassRepresentativeEnsure.Query(authorId, classId), ct);
            if (representative.IsFailure)
            {
                return representative;
            }

            await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.SchoolYearId), ct);

            var announcement = validation.Value;
            if (scope == AnnouncementScope.School)
            {
                var school = await queryBus.Send(new SchoolIdFinderBySchoolYear.Query(schoolClass.SchoolYearId), ct);

                var creation = Announcement.Create(
                    command.Id,
                    scope,
                    command.ClassId,
                    school,
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

                announcement = creation.Value;
            }

            await repository.AddAsync(announcement, ct);

            return Result.Success();
        }
    }

    public sealed class Handler(Create create) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => create.ExecuteAsync(command, ct);
    }
}
