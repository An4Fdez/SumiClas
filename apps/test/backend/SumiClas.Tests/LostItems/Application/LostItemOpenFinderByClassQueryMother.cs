namespace SumiClas.Tests.LostItems.Application;

using SumiClas.LostItems.Application;

public static class LostItemOpenFinderByClassQueryMother
{
    public static LostItemOpenFinderByClass.Query WithClassId(Guid classId) => new(classId);
}
