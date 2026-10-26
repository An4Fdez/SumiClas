using SumiClas.Shared.Domain;

namespace SumiClas.Announcements.Domain;

public static class AnnouncementExceptions
{
    public sealed class NotFound() : DomainException("Announcement.NotFound", "No se ha encontrado el aviso.");

    public sealed class IdRequired()
        : DomainException("Announcement.IdRequired", "El identificador del aviso no puede estar vacío.");

    public sealed class InvalidScope()
        : DomainException("Announcement.InvalidScope", "El alcance del aviso no es válido.");

    public sealed class InvalidPriority()
        : DomainException("Announcement.InvalidPriority", "La prioridad del aviso no es válida.");

    public sealed class SchoolRequiredForSchoolScope()
        : DomainException(
            "Announcement.SchoolRequiredForSchoolScope",
            "Un aviso con alcance de colegio debe indicar el colegio.");

    public sealed class ClassRequiredForSchoolScope()
        : DomainException(
            "Announcement.ClassRequiredForSchoolScope",
            "Un aviso con alcance de colegio debe indicar la clase desde la que se publica.");

    public sealed class ClassRequiredForClassScope()
        : DomainException(
            "Announcement.ClassRequiredForClassScope",
            "Un aviso con alcance de clase debe indicar la clase.");

    public sealed class TitleRequired()
        : DomainException("Announcement.TitleRequired", "El título del aviso no puede estar vacío.");

    public sealed class BodyRequired()
        : DomainException("Announcement.BodyRequired", "El cuerpo del aviso no puede estar vacío.");

    public sealed class AuthorRequired()
        : DomainException("Announcement.AuthorRequired", "El aviso debe tener un autor.");

    public sealed class ChangeForbidden()
        : DomainException(
            "Announcement.ChangeForbidden",
            "Solo el autor del aviso puede modificarlo o eliminarlo.");

    public sealed class ReadReceiptUserRequired()
        : DomainException(
            "Announcement.ReadReceiptUserRequired",
            "La confirmación de lectura requiere un usuario.");
}
