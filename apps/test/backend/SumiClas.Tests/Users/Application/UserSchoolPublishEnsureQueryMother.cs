namespace SumiClas.Tests.Users.Application;

using SumiClas.Users.Application;

public static class UserSchoolPublishEnsureQueryMother
{
    public static UserSchoolPublishEnsure.Query With(Guid userId, Guid schoolId) => new(userId, schoolId);
}
