using MediatR;
using SumiClas.LostItems.Domain;
using SumiClas.Shared.Application;
using SumiClas.Users.Application;

namespace SumiClas.LostItems.Application;

public static class LostItemClaimFinderByItem
{
    /// <summary>
    /// The claims of the item, newest first: all of them to its author, and only the ones of the current user to any
    /// other member of the class. The author's response message is only shown on an accepted claim.
    /// </summary>
    public sealed record Query(Guid LostItemId) : IQuery<IReadOnlyCollection<LostItemClaimResponse>>;

    public sealed class Finder(ILostItemRepository repository, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
    {
        public async Task<IReadOnlyCollection<LostItemClaimResponse>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            Guid current = userProvider.GetCurrentUserId();

            LostItem lostItem = await repository.GetByIdAsync(query.LostItemId, ct)
                ?? throw new LostItemExceptions.NotFound();

            Guid classId = lostItem.ClassId ?? throw new LostItemExceptions.ClaimClassRequired();

            await queryBus.Send(new UserClassAccessEnsure.Query(current, classId), ct);

            List<LostItemClaim> visible = lostItem.Claims
                .Where(claim => lostItem.AuthorId == current || claim.ClaimantId == current)
                .OrderByDescending(claim => claim.CreatedAt)
                .ThenByDescending(claim => claim.Id, Pagination.IdComparer)
                .ToList();

            List<Guid> claimantIds = visible.Select(claim => claim.ClaimantId).Distinct().ToList();
            Dictionary<Guid, UserClassMemberResponse> claimants = (await queryBus.Send(
                    new UserClassMemberFinderByIds.Query(classId, claimantIds), ct))
                .ToDictionary(claimant => claimant.UserId);

            return visible
                .Select(claim => new LostItemClaimResponse(
                    claim.Id,
                    claim.ClaimantId,
                    claimants.GetValueOrDefault(claim.ClaimantId)?.FullName ?? string.Empty,
                    claimants.GetValueOrDefault(claim.ClaimantId)?.ClassRole,
                    claim.Message,
                    claim.Status.ToString(),
                    claim.Status == ClaimStatus.Accepted ? claim.ResponseMessage : null,
                    claim.CreatedAt,
                    claim.ClaimantId == current))
                .ToList();
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, IReadOnlyCollection<LostItemClaimResponse>>
    {
        public Task<IReadOnlyCollection<LostItemClaimResponse>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
