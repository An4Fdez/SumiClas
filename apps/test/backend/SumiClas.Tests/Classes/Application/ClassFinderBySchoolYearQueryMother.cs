namespace SumiClas.Tests.Classes.Application;

using SumiClas.Classes.Application;

public static class ClassFinderBySchoolYearQueryMother
{
    public static ClassFinderBySchoolYear.Query WithSchoolYearId(Guid schoolYearId) => new(schoolYearId);
}
