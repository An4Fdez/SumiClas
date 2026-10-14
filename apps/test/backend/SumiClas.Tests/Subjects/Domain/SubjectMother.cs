namespace SumiClas.Tests.Subjects.Domain;

using SumiClas.Subjects.Domain;
using SumiClas.Tests.Shared;

public static class SubjectMother
{
    public static Subject With(Guid id, Guid classId, string label) => new(id, classId, label);

    public static Subject InClass(Guid classId)
        => With(GuidMother.Random(), classId, StringMother.Random("subject"));
}
