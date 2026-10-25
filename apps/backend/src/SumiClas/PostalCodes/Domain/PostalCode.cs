using SumiClas.Shared.Domain;

namespace SumiClas.PostalCodes.Domain;

public sealed class PostalCode : AggregateRoot
{
    public const int Length = 5;

    public Guid LocalityId { get; private set; }
    public string Code { get; private set; }

    public PostalCode(Guid id, Guid localityId, string code) : base(id)
    {
        LocalityId = localityId;
        Code = code;
    }

    public static PostalCode Create(Guid id, Guid localityId, string code)
    {
        string trimmed = code?.Trim() ?? string.Empty;
        Validate(id, localityId, trimmed);

        return new PostalCode(id, localityId, trimmed);
    }

    private static void Validate(Guid id, Guid localityId, string trimmed)
    {
        if (id == Guid.Empty)
        {
            throw new PostalCodeExceptions.IdRequired();
        }
        if (localityId == Guid.Empty)
        {
            throw new PostalCodeExceptions.LocalityRequired();
        }
        if (trimmed.Length != Length || !trimmed.All(char.IsAsciiDigit))
        {
            throw new PostalCodeExceptions.CodeInvalid();
        }
    }
}
