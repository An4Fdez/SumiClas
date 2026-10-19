using SumiClas.Shared.Domain;

namespace SumiClas.Announcements.Domain;

public static class AnnouncementErrors
{
    public static readonly Error IdRequired = new("Announcement.IdRequired", "El identificador del aviso no puede estar vacío.");
    public static readonly Error NotFound = new("Announcement.NotFound", "No se ha encontrado el aviso.");
    public static readonly Error InvalidScope = new("Announcement.InvalidScope", "El alcance del aviso no es válido.");
    public static readonly Error InvalidPriority = new("Announcement.InvalidPriority", "La prioridad del aviso no es válida.");
    public static readonly Error SchoolRequiredForSchoolScope = new("Announcement.SchoolRequiredForSchoolScope", "Un aviso con alcance de colegio debe indicar el colegio.");
    public static readonly Error ClassRequiredForSchoolScope = new("Announcement.ClassRequiredForSchoolScope", "Un aviso con alcance de colegio debe indicar la clase desde la que se publica.");
    public static readonly Error ClassRequiredForClassScope = new("Announcement.ClassRequiredForClassScope", "Un aviso con alcance de clase debe indicar la clase.");
    public static readonly Error TitleRequired = new("Announcement.TitleRequired", "El título del aviso no puede estar vacío.");
    public static readonly Error BodyRequired = new("Announcement.BodyRequired", "El cuerpo del aviso no puede estar vacío.");
    public static readonly Error AuthorRequired = new("Announcement.AuthorRequired", "El aviso debe tener un autor.");
    public static readonly Error ReadReceiptUserRequired = new("Announcement.ReadReceiptUserRequired", "La confirmación de lectura requiere un usuario.");
}
