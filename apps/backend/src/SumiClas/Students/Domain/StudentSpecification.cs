using SumiClas.Shared.Domain;

namespace SumiClas.Students.Domain;

public sealed class StudentSpecification : Specification<Student>
{
    public StudentSpecification(
        Guid? id = null,
        string? firstName = null,
        string? firstSurname = null,
        string? secondSurname = null,
        Guid? guardianId = null)
    {
        if (id is { } studentId) AddCriteria(student => student.Id == studentId);
        if (firstName is not null) AddCriteria(student => student.Name.FirstName == firstName);
        if (firstSurname is not null) AddCriteria(student => student.Name.FirstSurname == firstSurname);
        if (secondSurname is not null) AddCriteria(student => student.Name.SecondSurname == secondSurname);
        if (guardianId is { } guardian) AddCriteria(student => student.Guardians.Any(link => link.GuardianId == guardian));
    }
}
