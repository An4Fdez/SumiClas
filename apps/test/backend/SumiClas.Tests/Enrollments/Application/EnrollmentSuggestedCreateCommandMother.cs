namespace SumiClas.Tests.Enrollments.Application;

using SumiClas.Enrollments.Application;

public static class EnrollmentSuggestedCreateCommandMother
{
    public static EnrollmentSuggestedCreate.Command WithClasses(Guid classId, Guid previousClassId)
        => new(classId, previousClassId);
}
