namespace SumiClas.Tests.SchoolYears.Application;

using SumiClas.SchoolYears.Application;

public static class SchoolIdFinderBySchoolYearQueryMother
{
    public static SchoolIdFinderBySchoolYear.Query WithSchoolYearId(Guid schoolYearId) => new(schoolYearId);
}
