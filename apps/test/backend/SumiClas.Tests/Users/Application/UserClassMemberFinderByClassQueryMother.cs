namespace SumiClas.Tests.Users.Application;

using SumiClas.Users.Application;

public static class UserClassMemberFinderByClassQueryMother
{
    public static UserClassMemberFinderByClass.Query WithClassId(Guid classId) => new(classId);
}
