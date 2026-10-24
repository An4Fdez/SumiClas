namespace SumiClas.Tests.Dashboard.Application;

using SumiClas.Dashboard.Application;

public static class DashboardSummaryFinderByClassQueryMother
{
    public static DashboardSummaryFinderByClass.Query WithClassId(Guid classId) => new(classId);

    public static DashboardSummaryFinderByClass.Query WithDays(Guid classId, int days) => new(classId, days);
}
