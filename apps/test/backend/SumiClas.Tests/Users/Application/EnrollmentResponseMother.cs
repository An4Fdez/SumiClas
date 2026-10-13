namespace SumiClas.Tests.Users.Application;

using SumiClas.Enrollments.Application;
using SumiClas.Tests.Shared;

public static class EnrollmentResponseMother
{
    public const string CONFIRMED = "Confirmed";
    public const string SUGGESTED = "Suggested";

    public static EnrollmentResponse OfStatus(Guid classId, Guid studentId, string status)
        => new(GuidMother.Random(), studentId, classId, status, UnitTestCase.Now, null);
}
