namespace SumiClas.Tests.Classes.Application;

using SumiClas.Classes.Application;

public static class ClassFinderByIdsQueryMother
{
    public static ClassFinderByIds.Query WithIds(params Guid[] classIds) => new(classIds);
}
