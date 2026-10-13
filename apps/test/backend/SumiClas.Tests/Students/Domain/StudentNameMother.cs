namespace SumiClas.Tests.Students.Domain;

using SumiClas.Students.Domain;
using SumiClas.Tests.Shared;

public static class StudentNameMother
{
    public static StudentName Random()
        => StudentName.Create(StringMother.Random("First"), StringMother.Random("Surname"), null).Value;
}
