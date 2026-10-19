namespace SumiClas.Tests.Students.Application;

using SumiClas.Students.Application;

public static class StudentGuardianEnsureQueryMother
{
    public static StudentGuardianEnsure.Query With(Guid studentId, Guid userId) => new(studentId, userId);
}
