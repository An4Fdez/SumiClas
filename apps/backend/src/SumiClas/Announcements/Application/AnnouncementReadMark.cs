using MediatR;
using SumiClas.Announcements.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.Announcements.Application;

public static class AnnouncementReadMark
{
    /// <summary>The current user marks the announcement as read. Idempotent: marking it again does nothing.</summary>
    public sealed record Command(Guid AnnouncementId) : IRequest, ICommand;

    public sealed class Mark(IAnnouncementRepository repository, IQueryBus queryBus, IClock clock, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var current = userProvider.GetCurrentUserId();

            var announcement = await repository.GetByIdAsync(command.AnnouncementId, ct)
                ?? throw new AnnouncementExceptions.NotFound();

            await queryBus.Send(new UserFinderById.Query(current), ct);

            if (announcement.ClassId is { } classId)
            {
                await queryBus.Send(new UserClassAccessEnsure.Query(current, classId), ct);
            }

            announcement.MarkAsReadBy(current, clock);
        }
    }

    public sealed class Handler(Mark mark) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => mark.ExecuteAsync(command, ct);
    }
}
