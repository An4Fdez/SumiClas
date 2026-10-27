namespace SumiClas.Tests.LostItems.Application;

using SumiClas.LostItems.Application;

public static class LostItemClaimFinderByItemQueryMother
{
    public static LostItemClaimFinderByItem.Query With(Guid lostItemId) => new(lostItemId);
}
