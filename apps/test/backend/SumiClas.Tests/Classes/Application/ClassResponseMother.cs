namespace SumiClas.Tests.Classes.Application;

using SumiClas.Classes.Application;
using SumiClas.Tests.Shared;

public static class ClassResponseMother
{
    public static ClassResponse With(Guid id, Guid? schoolYearId = null)
        => new(id, schoolYearId ?? GuidMother.Random(), StringMother.Random("Class"));

    public static ClassResponse WithId(Guid id) => With(id);
}
