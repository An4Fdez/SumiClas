namespace SumiClas.Tests.Enrollments.Application;

using SumiClas.Enrollments.Application;

public static class EnrollmentFinderByClassQueryMother
{
    public static EnrollmentFinderByClass.Query WithClassId(Guid classId) => new(classId);
}
