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
        Guid? RelatedEventId) : IRequest, ICommand;

    public sealed class Create(IAnnouncementRepository repository, IQueryBus queryBus, IClock clock, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var current = userProvider.GetCurrentUserId();

            var authorId = current;

            if (!Enum.TryParse<AnnouncementScope>(command.Scope, ignoreCase: true, out var scope) || !Enum.IsDefined(scope))
            {
                throw new AnnouncementExceptions.InvalidScope();
            }
            if (!Enum.TryParse<Priority>(command.Priority, ignoreCase: true, out var priority) || !Enum.IsDefined(priority))
            {
                throw new AnnouncementExceptions.InvalidPriority();
            }

            // The school of a school-wide announcement is only known after looking up the class, so the aggregate is
            // built twice: once only to validate the input before any IO (the school is provisional and discarded),
            // and once with the real school.
            var announcement = Announcement.Create(
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

            if (command.ClassId is not { } classId || classId == Guid.Empty)
            {
                throw new AnnouncementExceptions.ClassRequiredForSchoolScope();
            }

            var author = await queryBus.Send(new UserFinderById.Query(authorId), ct);

            if (command.RelatedEventId is { } relatedEventId)
            {
                await queryBus.Send(new CalendarEventFinderById.Query(relatedEventId), ct);
            }

            var schoolClass = await queryBus.Send(new ClassFinderById.Query(classId), ct);

            await queryBus.Send(new UserClassRepresentativeEnsure.Query(authorId, classId), ct);

            await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.SchoolYearId), ct);

            if (scope == AnnouncementScope.School)
            {
                var school = await queryBus.Send(new SchoolIdFinderBySchoolYear.Query(schoolClass.SchoolYearId), ct);

                announcement = Announcement.Create(
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
            }

            await repository.AddAsync(announcement, ct);
        }
    }

    public sealed class Handler(Create create) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => create.ExecuteAsync(command, ct);
    }
}
