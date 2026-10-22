using System.Diagnostics.CodeAnalysis;
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

    public static Email Create(string value)
        => TryCreate(value, out var email) ? email : throw new UserExceptions.InvalidEmailFormat();

    /// <summary>For the callers that must treat a malformed email as "no such account" instead of failing.</summary>
    public static bool TryCreate(string? value, [NotNullWhen(true)] out Email? email)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed) || !EmailFormat().IsMatch(trimmed))
        {
            email = null;
            return false;
        }

        email = new Email(trimmed.ToLowerInvariant());
        return true;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailFormat();
}
