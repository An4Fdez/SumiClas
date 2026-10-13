namespace SumiClas.Tests.SchoolYears.Application;

using SumiClas.SchoolYears.Application;

public static class SchoolYearEnsureIsActiveQueryMother
{
    public static SchoolYearEnsureIsActive.Query WithSchoolYearId(Guid schoolYearId) => new(schoolYearId);
}
