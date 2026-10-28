using MediatR;
using SumiClas.Announcements.Domain;
using SumiClas.CalendarEvents.Application;
using SumiClas.Classes.Application;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.Announcements.Application;

public static class AnnouncementUpdate
{
    /// <summary>
    /// The author changes the title, the text, the priority and the related event. The scope, the class and the date
    /// of publication stay. The announcement is unread again for everybody.
    /// </summary>
    public sealed record Command(
        Guid Id,
        string Title,
        string Body,
        string Priority,
        Guid? RelatedEventId) : IRequest, ICommand;

    public sealed class Update(IAnnouncementRepository repository, IQueryBus queryBus, IClock clock, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            Guid current = userProvider.GetCurrentUserId();

            Announcement announcement = await repository.GetByIdAsync(command.Id, ct)
                ?? throw new AnnouncementExceptions.NotFound();

            if (announcement.AuthorId != current)
            {
                throw new AnnouncementExceptions.ChangeForbidden();
            }

            Announcement.ValidateContent(command.Title, command.Body);

            if (!Enum.TryParse<Priority>(command.Priority, ignoreCase: true, out Priority priority) || !Enum.IsDefined(priority))
            {
                throw new AnnouncementExceptions.InvalidPriority();
            }

            if (command.RelatedEventId is { } relatedEventId)
            {
                await queryBus.Send(new CalendarEventFinderById.Query(relatedEventId), ct);
            }

            if (announcement.ClassId is { } classId)
            {
                ClassResponse schoolClass = await queryBus.Send(new ClassFinderById.Query(classId), ct);

                await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.SchoolYearId), ct);
            }

            announcement.Update(command.Title, command.Body, priority, command.RelatedEventId, clock);
        }
    }

    public sealed class Handler(Update update) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => update.ExecuteAsync(command, ct);
    }
}
