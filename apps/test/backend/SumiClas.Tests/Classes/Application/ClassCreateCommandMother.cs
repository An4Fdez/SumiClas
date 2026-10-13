namespace SumiClas.Tests.Classes.Application;

using SumiClas.Classes.Application;

public static class ClassCreateCommandMother
{
    public static ClassCreate.Command With(Guid id, Guid schoolYearId, string name) => new(id, schoolYearId, name);
}
