using SumiClas.Shared.Domain;

namespace SumiClas.Announcements.Domain;

public static class AnnouncementErrors
{
    public static readonly Error ClassRequiredForClassScope = new("Announcement.ClassRequiredForClassScope", "Un aviso con alcance de clase debe indicar la clase.");
    public static readonly Error TitleRequired = new("Announcement.TitleRequired", "El título del aviso no puede estar vacío.");
    public static readonly Error BodyRequired = new("Announcement.BodyRequired", "El cuerpo del aviso no puede estar vacío.");
    public static readonly Error AuthorRequired = new("Announcement.AuthorRequired", "El aviso debe tener un autor.");
    public static readonly Error ReadReceiptUserRequired = new("Announcement.ReadReceiptUserRequired", "La confirmación de lectura requiere un usuario.");
}
