namespace SumiClas.Tests.Users.Application;

using SumiClas.Users.Application;

public static class UserAnyClassRepresentativeEnsureQueryMother
{
    public static UserAnyClassRepresentativeEnsure.Query With(Guid userId, params Guid[] classIds)
        => new(userId, classIds);
}
