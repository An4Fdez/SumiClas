using MediatR;
using SumiClas.Classes.Application;
using SumiClas.LostItems.Domain;
using SumiClas.Shared.Application;
using SumiClas.Users.Application;

namespace SumiClas.LostItems.Application;

public static class LostItemClaimPendingSummaryFinderByClass
{
    /// <summary>
    /// How many claims are waiting for the current user to accept or reject them, on the open items of the class that
    /// the user published.
    /// </summary>
    public sealed record Query(Guid ClassId) : IQuery<LostItemClaimPendingSummaryResponse>;

    public sealed class Finder(ILostItemRepository repository, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
    {
        public async Task<LostItemClaimPendingSummaryResponse> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            Guid current = userProvider.GetCurrentUserId();

            await queryBus.Send(new ClassFinderById.Query(query.ClassId), ct);

            await queryBus.Send(new UserClassAccessEnsure.Query(current, query.ClassId), ct);

            int count = (await repository.SearchAsync(
                    new LostItemSpecification(scope: ItemScope.Class, classId: query.ClassId, recovered: false, authorId: current), ct))
                .Sum(lostItem => lostItem.CountPendingClaims());

            return new LostItemClaimPendingSummaryResponse(count);
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, LostItemClaimPendingSummaryResponse>
    {
        public Task<LostItemClaimPendingSummaryResponse> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
