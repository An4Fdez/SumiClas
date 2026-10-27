namespace SumiClas.Tests.LostItems.Domain;

using SumiClas.LostItems.Domain;
using SumiClas.Tests.Shared;

public static class LostItemClaimMother
{
    public static LostItemClaim With(
        Guid? id = null,
        Guid? claimantId = null,
        string? message = null,
        ClaimStatus status = ClaimStatus.Pending,
        string? responseMessage = null,
        DateTimeOffset? createdAt = null)
        => new(
            id ?? GuidMother.Random(),
            claimantId ?? GuidMother.Random(),
            message ?? StringMother.Random("claim"),
            status,
            responseMessage,
            createdAt ?? UnitTestCase.Now);

    public static LostItemClaim PendingOf(Guid claimantId, DateTimeOffset? createdAt = null)
        => With(claimantId: claimantId, createdAt: createdAt);
}
