using MediatR;
using SumiClas.Classes.Application;
using SumiClas.LostItems.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.LostItems.Application;

public static class LostItemNotRecoveredMark
{
    public sealed record Command(Guid LostItemId) : IRequest, ICommand;

    public sealed class Mark(ILostItemRepository repository, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            LostItem lostItem = await repository.GetByIdAsync(command.LostItemId, ct)
                ?? throw new LostItemExceptions.NotFound();

            Guid current = userProvider.GetCurrentUserId();

            if (lostItem.AuthorId != current)
            {
                throw new LostItemExceptions.MarkForbidden();
            }

            if (lostItem.ClassId is { } classId)
            {
                ClassResponse schoolClass = await queryBus.Send(new ClassFinderById.Query(classId), ct);

                await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.SchoolYearId), ct);
            }

            lostItem.MarkAsNotRecovered();
        }
    }

    public sealed class Handler(Mark mark) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => mark.ExecuteAsync(command, ct);
    }
}
