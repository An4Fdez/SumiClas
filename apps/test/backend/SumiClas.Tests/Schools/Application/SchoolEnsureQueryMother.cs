namespace SumiClas.Tests.Schools.Application;

using SumiClas.Schools.Application;
using SumiClas.Tests.Shared;

public static class SchoolEnsureQueryMother
{
    public static SchoolEnsure.Query Random() => With();

    public static SchoolEnsure.Query With(Guid? schoolId = null) => new(schoolId ?? GuidMother.Random());

    public static SchoolEnsure.Query WithSchoolId(Guid schoolId) => With(schoolId);
}
