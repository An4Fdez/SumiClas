using MediatR;
using SumiClas.Announcements.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.Announcements.Application;

public static class AnnouncementReadMark
{
    /// <summary>The current user marks the announcement as read. Idempotent: marking it again does nothing.</summary>
    public sealed record Command(Guid AnnouncementId) : IRequest<Result>, ICommand;

    public sealed class UseCase(IAnnouncementRepository announcements, IQueryBus queryBus, IClock clock, IUserProvider userProvider)
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var current = userProvider.GetCurrentUserId();
            if (current.IsFailure)
            {
                return Result.Failure(current.Error!);
            }

            var announcement = await announcements.GetByIdAsync(command.AnnouncementId, ct);
            if (announcement is null)
            {
                return Result.Failure(AnnouncementErrors.NotFound);
            }

            var reader = await queryBus.Send(new UserFinderById.Query(current.Value), ct);
            if (reader.IsFailure)
            {
                return Result.Failure(reader.Error!);
            }

            return announcement.MarkAsReadBy(current.Value, clock);
        }
    }

    public sealed class Handler(UseCase useCase) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => useCase.ExecuteAsync(command, ct);
    }
}
