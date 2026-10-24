namespace SumiClas.Tests.LostItems.Application;

using SumiClas.LostItems.Application;

public static class LostItemRecentSummaryFinderByClassQueryMother
{
    public static LostItemRecentSummaryFinderByClass.Query WithClassId(Guid classId) => new(classId);

    public static LostItemRecentSummaryFinderByClass.Query WithDays(Guid classId, int days) => new(classId, days);
}
