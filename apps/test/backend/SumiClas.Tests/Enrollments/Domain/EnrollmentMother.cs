namespace SumiClas.Tests.Enrollments.Domain;

using SumiClas.Enrollments.Domain;
using SumiClas.Tests.Shared;

public static class EnrollmentMother
{
    public static Enrollment Confirmed(Guid studentId, Guid classId)
        => With(studentId, classId, EnrollmentStatus.Confirmed);

    public static Enrollment Suggested(Guid studentId, Guid classId)
        => With(studentId, classId, EnrollmentStatus.Suggested);

    private static Enrollment With(Guid studentId, Guid classId, EnrollmentStatus status)
        => new(
            GuidMother.Random(),
            studentId,
            classId,
            status,
            UnitTestCase.Now,
            status == EnrollmentStatus.Confirmed ? UnitTestCase.Now : null);
}
