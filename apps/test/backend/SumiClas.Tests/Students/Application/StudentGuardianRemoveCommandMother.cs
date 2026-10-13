namespace SumiClas.Tests.Students.Application;

using SumiClas.Students.Application;

public static class StudentGuardianRemoveCommandMother
{
    public static StudentGuardianRemove.Command With(Guid studentId, Guid guardianId) => new(studentId, guardianId);
}
