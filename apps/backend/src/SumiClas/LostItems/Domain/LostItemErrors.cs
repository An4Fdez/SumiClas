using SumiClas.Shared.Domain;

namespace SumiClas.LostItems.Domain;

public static class LostItemErrors
{
    public static readonly Error ClassRequiredForClassScope = new("LostItem.ClassRequiredForClassScope", "Un objeto con alcance de clase debe indicar la clase.");
    public static readonly Error DescriptionRequired = new("LostItem.DescriptionRequired", "La descripción no puede estar vacía.");
    public static readonly Error PhotoPrivacyConfirmationRequired = new("LostItem.PhotoPrivacyConfirmationRequired", "Antes de adjuntar una foto hay que confirmar que no muestra a ningún menor.");
    public static readonly Error AuthorRequired = new("LostItem.AuthorRequired", "El objeto debe tener un autor.");
}
