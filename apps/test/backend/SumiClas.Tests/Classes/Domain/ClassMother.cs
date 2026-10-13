namespace SumiClas.Tests.Classes.Domain;

using SumiClas.Classes.Domain;
using SumiClas.Tests.Shared;

public static class ClassMother
{
    public static Class With(Guid id, Guid schoolYearId, string name) => new(id, schoolYearId, name);

    public static Class InSchoolYear(Guid schoolYearId)
        => With(GuidMother.Random(), schoolYearId, StringMother.Random("class"));
}
