namespace SumiClas.Tests.Classes.Application;

using SumiClas.Classes.Application;

public static class ClassFinderByIdQueryMother
{
    public static ClassFinderById.Query WithClassId(Guid classId) => new(classId);
}
