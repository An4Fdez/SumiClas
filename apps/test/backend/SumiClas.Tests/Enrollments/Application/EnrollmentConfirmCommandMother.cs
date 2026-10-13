namespace SumiClas.Tests.Enrollments.Application;

using SumiClas.Enrollments.Application;

public static class EnrollmentConfirmCommandMother
{
    public static EnrollmentConfirm.Command WithEnrollmentId(Guid enrollmentId) => new(enrollmentId);
}
