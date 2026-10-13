namespace SumiClas.Tests.SchoolYears.Application;

using SumiClas.SchoolYears.Application;

public static class SchoolYearFinderBySchoolQueryMother
{
    public static SchoolYearFinderBySchool.Query With(Guid schoolId, string? status) => new(schoolId, status);

    public static SchoolYearFinderBySchool.Query WithSchoolId(Guid schoolId) => With(schoolId, null);
}
