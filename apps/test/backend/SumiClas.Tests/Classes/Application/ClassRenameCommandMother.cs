namespace SumiClas.Tests.Classes.Application;

using SumiClas.Classes.Application;

public static class ClassRenameCommandMother
{
    public static ClassRename.Command With(Guid classId, string name) => new(classId, name);
}
