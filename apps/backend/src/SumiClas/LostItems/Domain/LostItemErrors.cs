using SumiClas.Shared.Domain;

namespace SumiClas.LostItems.Domain;

public static class LostItemErrors
{
    public static readonly Error IdRequired = new("LostItem.IdRequired", "El identificador del objeto no puede estar vacío.");
    public static readonly Error MarkForbidden = new("LostItem.MarkForbidden", "Solo el autor del objeto perdido puede cambiar su estado.");
    public static readonly Error NotFound = new("LostItem.NotFound", "No se ha encontrado el objeto.");
    public static readonly Error InvalidScope = new("LostItem.InvalidScope", "El alcance del objeto no es válido.");
    public static readonly Error InvalidStatus = new("LostItem.InvalidStatus", "El estado del objeto no es válido.");
    public static readonly Error SchoolRequiredForSchoolScope = new("LostItem.SchoolRequiredForSchoolScope", "Un objeto con alcance de colegio debe indicar el colegio.");
    public static readonly Error ClassRequiredForSchoolScope = new("LostItem.ClassRequiredForSchoolScope", "Un objeto con alcance de colegio debe indicar la clase desde la que se publica.");
    public static readonly Error ClassRequiredForClassScope = new("LostItem.ClassRequiredForClassScope", "Un objeto con alcance de clase debe indicar la clase.");
    public static readonly Error DescriptionRequired = new("LostItem.DescriptionRequired", "La descripción no puede estar vacía.");
    public static readonly Error PhotoPrivacyConfirmationRequired = new("LostItem.PhotoPrivacyConfirmationRequired", "Antes de adjuntar una foto hay que confirmar que no muestra a ningún menor.");
    public static readonly Error AuthorRequired = new("LostItem.AuthorRequired", "El objeto debe tener un autor.");
}
