namespace SumiClas.Tests.LostItems.Application;

using SumiClas.LostItems.Application;

public static class LostItemClaimAcceptCommandMother
{
    public static LostItemClaimAccept.Command With(Guid lostItemId, Guid claimId, string? responseMessage = null)
        => new(lostItemId, claimId, responseMessage);
}
