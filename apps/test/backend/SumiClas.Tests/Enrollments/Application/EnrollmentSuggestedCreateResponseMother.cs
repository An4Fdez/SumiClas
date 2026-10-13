namespace SumiClas.Tests.Enrollments.Application;

using SumiClas.Enrollments.Application;

public static class EnrollmentSuggestedCreateResponseMother
{
    public static EnrollmentSuggestedCreateResponse Create(int created, int skipped) => new(created, skipped);

    public static EnrollmentSuggestedCreateResponse Nothing() => Create(0, 0);
}
