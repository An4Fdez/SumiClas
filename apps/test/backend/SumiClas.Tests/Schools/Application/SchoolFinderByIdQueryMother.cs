namespace SumiClas.Tests.Schools.Application;

using SumiClas.Schools.Application;
using SumiClas.Tests.Shared;

public static class SchoolFinderByIdQueryMother
{
    public static SchoolFinderById.Query Random() => With();

    public static SchoolFinderById.Query With(Guid? schoolId = null) => new(schoolId ?? GuidMother.Random());

    public static SchoolFinderById.Query WithSchoolId(Guid schoolId) => With(schoolId);
}
