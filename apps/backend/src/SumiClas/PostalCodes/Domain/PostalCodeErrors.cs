using SumiClas.Shared.Domain;

namespace SumiClas.PostalCodes.Domain;

public static class PostalCodeErrors
{
    public static readonly Error NotFound = new("PostalCode.NotFound", "No se ha encontrado el código postal.");
    public static readonly Error IdRequired = new("PostalCode.IdRequired", "El identificador del código postal no puede estar vacío.");
    public static readonly Error LocalityRequired = new("PostalCode.LocalityRequired", "El código postal debe pertenecer a una población.");
    public static readonly Error CodeInvalid = new("PostalCode.CodeInvalid", "El código postal debe tener 5 dígitos.");
}
