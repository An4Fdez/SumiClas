using MediatR;
using SumiClas.Classes.Application;
using SumiClas.LostItems.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;

namespace SumiClas.LostItems.Application;

public static class LostItemClaimReject
{
    /// <summary>The author rejects a claim; the item stays open for the other claims.</summary>
    public sealed record Command(Guid LostItemId, Guid ClaimId) : IRequest, ICommand;

    public sealed class Reject(ILostItemRepository repository, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            Guid current = userProvider.GetCurrentUserId();

            LostItem lostItem = await repository.GetByIdAsync(command.LostItemId, ct)
                ?? throw new LostItemExceptions.NotFound();

            if (lostItem.AuthorId != current)
            {
                throw new LostItemExceptions.ClaimResolveForbidden();
            }

            if (lostItem.ClassId is { } classId)
            {
                ClassResponse schoolClass = await queryBus.Send(new ClassFinderById.Query(classId), ct);

                await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.SchoolYearId), ct);
            }

            lostItem.RejectClaim(command.ClaimId);
        }
    }

    public sealed class Handler(Reject reject) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => reject.ExecuteAsync(command, ct);
    }
}
