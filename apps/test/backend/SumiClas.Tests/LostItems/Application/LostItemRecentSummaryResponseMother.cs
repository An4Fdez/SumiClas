namespace SumiClas.Tests.LostItems.Application;

using SumiClas.LostItems.Application;

public static class LostItemRecentSummaryResponseMother
{
    public static LostItemRecentSummaryResponse None() => new(Count: 0);
}
