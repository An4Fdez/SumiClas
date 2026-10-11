using MediatR;
using SumiClas.Classes.Application;
using SumiClas.LostItems.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.LostItems.Application;

public static class LostItemNotRecoveredMark
{
    public sealed record Command(Guid LostItemId) : IRequest<Result>, ICommand;

    public sealed class Mark(ILostItemRepository repository, IQueryBus queryBus) : IUseCase
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var lostItem = await repository.GetByIdAsync(command.LostItemId, ct);
            if (lostItem is null)
            {
                return Result.Failure(LostItemErrors.NotFound);
            }

            if (lostItem.ClassId is { } classId)
            {
                var schoolClass = await queryBus.Send(new ClassFinderById.Query(classId), ct);
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

            lostItem.MarkAsNotRecovered();
            return Result.Success();
        }
    }

    public sealed class Handler(Mark mark) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => mark.ExecuteAsync(command, ct);
    }
}
