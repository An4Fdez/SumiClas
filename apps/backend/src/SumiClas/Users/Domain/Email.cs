using System.Text.RegularExpressions;
using SumiClas.Shared.Domain;

namespace SumiClas.Users.Domain;

public sealed partial class Email : ValueObject
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Result<Email> Create(string value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed) || !EmailFormat().IsMatch(trimmed))
        {
            return Result<Email>.Failure(UserErrors.InvalidEmailFormat);
        }

        return Result<Email>.Success(new Email(trimmed.ToLowerInvariant()));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailFormat();
}
