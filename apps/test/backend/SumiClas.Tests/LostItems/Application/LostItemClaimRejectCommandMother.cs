namespace SumiClas.Tests.LostItems.Application;

using SumiClas.LostItems.Application;

public static class LostItemClaimRejectCommandMother
{
    public static LostItemClaimReject.Command With(Guid lostItemId, Guid claimId) => new(lostItemId, claimId);
}
