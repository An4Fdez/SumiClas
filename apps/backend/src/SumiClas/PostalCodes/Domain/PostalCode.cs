using SumiClas.Shared.Domain;

namespace SumiClas.PostalCodes.Domain;

public sealed class PostalCode : AggregateRoot
{
    public const int Length = 5;

    public Guid LocalityId { get; private set; }
    public string Code { get; private set; }

    private PostalCode(Guid id, Guid localityId, string code) : base(id)
    {
        LocalityId = localityId;
        Code = code;
    }

    public static Result<PostalCode> Create(Guid id, Guid localityId, string code)
    {
        if (id == Guid.Empty)
        {
            return Result<PostalCode>.Failure(PostalCodeErrors.IdRequired);
        }
        if (localityId == Guid.Empty)
        {
            return Result<PostalCode>.Failure(PostalCodeErrors.LocalityRequired);
        }

        var trimmed = code?.Trim() ?? string.Empty;
        if (trimmed.Length != Length || !trimmed.All(char.IsAsciiDigit))
        {
            return Result<PostalCode>.Failure(PostalCodeErrors.CodeInvalid);
        }

        return Result<PostalCode>.Success(new PostalCode(id, localityId, trimmed));
    }
}
