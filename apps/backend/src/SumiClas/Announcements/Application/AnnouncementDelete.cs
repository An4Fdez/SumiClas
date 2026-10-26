using MediatR;
using SumiClas.Announcements.Domain;
using SumiClas.Classes.Application;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;

namespace SumiClas.Announcements.Application;

public static class AnnouncementDelete
{
    /// <summary>The author deletes the announcement for good, with the receipts of whoever had read it.</summary>
    public sealed record Command(Guid Id) : IRequest, ICommand;

    public sealed class Delete(IAnnouncementRepository repository, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
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

            if (announcement.ClassId is { } classId)
            {
                ClassResponse schoolClass = await queryBus.Send(new ClassFinderById.Query(classId), ct);

                await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.SchoolYearId), ct);
            }

            repository.Remove(announcement);
        }
    }

    public sealed class Handler(Delete delete) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => delete.ExecuteAsync(command, ct);
    }
}
