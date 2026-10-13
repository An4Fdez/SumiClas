namespace SumiClas.Tests.Enrollments.Application;

using SumiClas.Enrollments.Application;

public static class EnrollmentCreateCommandMother
{
    public static EnrollmentCreate.Command With(Guid id, Guid studentId, Guid classId) => new(id, studentId, classId);
}
