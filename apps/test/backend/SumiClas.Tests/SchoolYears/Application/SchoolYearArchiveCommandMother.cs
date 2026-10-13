namespace SumiClas.Tests.SchoolYears.Application;

using SumiClas.SchoolYears.Application;

public static class SchoolYearArchiveCommandMother
{
    public static SchoolYearArchive.Command WithSchoolYearId(Guid schoolYearId) => new(schoolYearId);
}
