namespace SumiClas.Tests.Students.Application;

using SumiClas.Classes.Application;
using SumiClas.Schools.Application;
using SumiClas.Students.Application;

public static class ChildResponseMother
{
    public static ChildResponse WithoutClass(
        Guid studentId,
        string firstName,
        string surnameInitial,
        string firstSurname,
        string? secondSurname)
        => new(studentId, firstName, surnameInitial, firstSurname, secondSurname, null, null, null, null, null);

    public static ChildResponse InClass(
        Guid studentId,
        string firstName,
        string surnameInitial,
        string firstSurname,
        string? secondSurname,
        ClassResponse schoolClass)
        => new(
            studentId,
            firstName,
            surnameInitial,
            firstSurname,
            secondSurname,
            schoolClass.Id,
            schoolClass.Name,
            schoolClass.SchoolYearId,
            null,
            null);

    public static ChildResponse InSchool(
        Guid studentId,
        string firstName,
        string surnameInitial,
        string firstSurname,
        string? secondSurname,
        ClassResponse schoolClass,
        SchoolResponse school)
        => new(
            studentId,
            firstName,
            surnameInitial,
            firstSurname,
            secondSurname,
            schoolClass.Id,
            schoolClass.Name,
            schoolClass.SchoolYearId,
            school.Id,
            school.Name);
}
