namespace SumiClas.Tests.Enrollments.Application;

using SumiClas.Enrollments.Application;
using SumiClas.Enrollments.Domain;
using SumiClas.Tests.Shared;

public static class EnrollmentResponseMother
{
    public static EnrollmentResponse Confirmed(Enrollment enrollment)
        => new(
            enrollment.Id,
            enrollment.StudentId,
            enrollment.ClassId,
            "Confirmed",
            enrollment.CreatedAt,
            enrollment.ConfirmedAt);

    public static EnrollmentResponse Suggested(Enrollment enrollment)
        => new(
            enrollment.Id,
            enrollment.StudentId,
            enrollment.ClassId,
            "Suggested",
            enrollment.CreatedAt,
            enrollment.ConfirmedAt);

    public static EnrollmentResponse ConfirmedFor(Guid studentId, Guid classId)
        => new(GuidMother.Random(), studentId, classId, "Confirmed", UnitTestCase.Now, UnitTestCase.Now);

    public static EnrollmentResponse SuggestedFor(Guid studentId, Guid classId)
        => new(GuidMother.Random(), studentId, classId, "Suggested", UnitTestCase.Now, null);
}
