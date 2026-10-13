namespace SumiClas.Tests.SchoolYears.Application;

using SumiClas.SchoolYears.Application;

public static class SchoolYearFinderByIdsQueryMother
{
    public static SchoolYearFinderByIds.Query WithIds(params Guid[] schoolYearIds) => new(schoolYearIds);
}
