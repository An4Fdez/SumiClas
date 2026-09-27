using SumiClas.Shared.Domain;

namespace SumiClas.Students.Domain;

public sealed class StudentName : ValueObject
{
    public string FirstName { get; }
    public string FirstSurname { get; }
    public string? SecondSurname { get; }

    private StudentName(string firstName, string firstSurname, string? secondSurname)
    {
        FirstName = firstName;
        FirstSurname = firstSurname;
        SecondSurname = secondSurname;
    }

    public static Result<StudentName> Create(string firstName, string firstSurname, string? secondSurname = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            return Result<StudentName>.Failure(StudentErrors.NameRequired);
        }
        if (string.IsNullOrWhiteSpace(firstSurname))
        {
            return Result<StudentName>.Failure(StudentErrors.FirstSurnameRequired);
        }

        var normalizedSecondSurname = string.IsNullOrWhiteSpace(secondSurname) ? null : secondSurname.Trim();
        return Result<StudentName>.Success(new StudentName(firstName.Trim(), firstSurname.Trim(), normalizedSecondSurname));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FirstName;
        yield return FirstSurname;
        yield return SecondSurname;
    }
}
