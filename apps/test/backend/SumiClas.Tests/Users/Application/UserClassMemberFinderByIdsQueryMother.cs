namespace SumiClas.Tests.Users.Application;

using SumiClas.Users.Application;

public static class UserClassMemberFinderByIdsQueryMother
{
    public static UserClassMemberFinderByIds.Query WithUserIds(Guid classId, params Guid[] userIds)
        => new(classId, userIds);
}
