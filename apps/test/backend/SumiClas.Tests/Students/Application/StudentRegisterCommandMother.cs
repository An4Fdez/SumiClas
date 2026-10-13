namespace SumiClas.Tests.Students.Application;

using SumiClas.Students.Application;
using SumiClas.Students.Domain;
using SumiClas.Tests.Shared;

public static class StudentRegisterCommandMother
{
    public static StudentRegister.Command With(
        Guid id,
        string firstName,
        string firstSurname,
        string? secondSurname,
        string firstGuardianRole)
        => new(id, firstName, firstSurname, secondSurname, firstGuardianRole);

    public static StudentRegister.Command WithId(Guid id)
        => new(
            id,
            StringMother.Random("First"),
            StringMother.Random("Surname"),
            null,
            nameof(GuardianRole.Mother));

    public static StudentRegister.Command WithRole(string firstGuardianRole)
        => new(
            GuidMother.Random(),
            StringMother.Random("First"),
            StringMother.Random("Surname"),
            null,
            firstGuardianRole);

    public static StudentRegister.Command WithName(string firstName, string firstSurname)
        => new(GuidMother.Random(), firstName, firstSurname, null, nameof(GuardianRole.Mother));
}
