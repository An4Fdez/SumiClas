namespace SumiClas.Tests.LostItems.Application;

using SumiClas.LostItems.Application;

public static class LostItemClaimPendingSummaryFinderByClassQueryMother
{
    public static LostItemClaimPendingSummaryFinderByClass.Query WithClassId(Guid classId) => new(classId);
}
