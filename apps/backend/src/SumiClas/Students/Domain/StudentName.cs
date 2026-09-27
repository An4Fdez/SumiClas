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
        var validation = Validate(firstName, firstSurname);
        if (validation.IsFailure)
        {
            return Result<StudentName>.Failure(validation.Error!);
        }

        var normalizedSecondSurname = string.IsNullOrWhiteSpace(secondSurname) ? null : secondSurname.Trim();
        return Result<StudentName>.Success(new StudentName(firstName.Trim(), firstSurname.Trim(), normalizedSecondSurname));
    }

    private static Result Validate(string firstName, string firstSurname)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            return Result.Failure(StudentErrors.NameRequired);
        }
        if (string.IsNullOrWhiteSpace(firstSurname))
        {
            return Result.Failure(StudentErrors.FirstSurnameRequired);
        }
        return Result.Success();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FirstName;
        yield return FirstSurname;
        yield return SecondSurname;
    }
}
