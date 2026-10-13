namespace SumiClas.Tests.Users.Application;

using SumiClas.Users.Application;

public static class UserClassMemberFinderByIdsQueryMother
{
    public static UserClassMemberFinderByIds.Query With(Guid classId, params Guid[] userIds) => new(classId, userIds);
}
