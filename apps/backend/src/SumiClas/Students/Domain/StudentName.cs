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

    public static StudentName Create(string firstName, string firstSurname, string? secondSurname = null)
    {
        Validate(firstName, firstSurname);

        var normalizedSecondSurname = string.IsNullOrWhiteSpace(secondSurname) ? null : secondSurname.Trim();
        return new StudentName(firstName.Trim(), firstSurname.Trim(), normalizedSecondSurname);
    }

    private static void Validate(string firstName, string firstSurname)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new StudentExceptions.NameRequired();
        }
        if (string.IsNullOrWhiteSpace(firstSurname))
        {
            throw new StudentExceptions.FirstSurnameRequired();
        }
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FirstName;
        yield return FirstSurname;
        yield return SecondSurname;
    }
}
