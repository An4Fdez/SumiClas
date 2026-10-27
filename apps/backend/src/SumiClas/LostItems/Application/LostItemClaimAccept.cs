using MediatR;
using SumiClas.Classes.Application;
using SumiClas.LostItems.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;

namespace SumiClas.LostItems.Application;

public static class LostItemClaimAccept
{
    /// <summary>
    /// The author accepts a claim, optionally leaving a line to coordinate the handover. The item becomes recovered and
    /// the other pending claims are rejected.
    /// </summary>
    public sealed record Command(Guid LostItemId, Guid ClaimId, string? ResponseMessage) : IRequest, ICommand;

    public sealed class Accept(ILostItemRepository repository, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
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

            lostItem.AcceptClaim(command.ClaimId, command.ResponseMessage);
        }
    }

    public sealed class Handler(Accept accept) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => accept.ExecuteAsync(command, ct);
    }
}
