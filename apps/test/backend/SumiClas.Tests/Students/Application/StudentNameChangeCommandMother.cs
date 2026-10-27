namespace SumiClas.Tests.Students.Application;

using SumiClas.Students.Application;
using SumiClas.Tests.Shared;

public static class StudentNameChangeCommandMother
{
    public static StudentNameChange.Command With(
        Guid studentId,
        string? firstName = null,
        string? firstSurname = null,
        string? secondSurname = null)
        => new(
            studentId,
            firstName ?? StringMother.Random("First"),
            firstSurname ?? StringMother.Random("Surname"),
            secondSurname);
}
