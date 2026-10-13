namespace SumiClas.Tests.Users.Application;

using SumiClas.Users.Application;

public static class UserClassMemberResponseMother
{
    public static UserClassMemberResponse Representative(Guid userId, string fullName)
        => new(userId, fullName, "ClassRepresentative");
}
