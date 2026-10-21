using MediatR;
using SumiClas.Classes.Application;
using SumiClas.LostItems.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.LostItems.Application;

public static class LostItemRecoveredMark
{
    public sealed record Command(Guid LostItemId) : IRequest<Result>, ICommand;

    public sealed class Mark(ILostItemRepository repository, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var lostItem = await repository.GetByIdAsync(command.LostItemId, ct);
            if (lostItem is null)
            {
                return Result.Failure(LostItemErrors.NotFound);
            }

            var current = userProvider.GetCurrentUserId();

            if (lostItem.AuthorId != current)
            {
                return Result.Failure(LostItemErrors.MarkForbidden);
            }

            if (lostItem.ClassId is { } classId)
            {
                var schoolClass = await queryBus.Send(new ClassFinderById.Query(classId), ct);

                await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.SchoolYearId), ct);
            }

            lostItem.MarkAsRecovered();
            return Result.Success();
        }
    }

    public sealed class Handler(Mark mark) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => mark.ExecuteAsync(command, ct);
    }
}
