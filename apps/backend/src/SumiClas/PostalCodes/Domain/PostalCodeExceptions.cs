using SumiClas.Shared.Domain;

namespace SumiClas.PostalCodes.Domain;

public static class PostalCodeExceptions
{
    public sealed class IdRequired()
        : DomainException("PostalCode.IdRequired", "El identificador del código postal no puede estar vacío.");

    public sealed class LocalityRequired()
        : DomainException("PostalCode.LocalityRequired", "El código postal debe pertenecer a una población.");

    public sealed class CodeInvalid()
        : DomainException("PostalCode.CodeInvalid", "El código postal debe tener 5 dígitos.");
}
