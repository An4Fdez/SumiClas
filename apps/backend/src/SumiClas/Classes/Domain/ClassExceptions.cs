using SumiClas.Shared.Domain;

namespace SumiClas.Classes.Domain;

public static class ClassExceptions
{
    public sealed class NotFound() : DomainException("Class.NotFound", "No se ha encontrado la clase.");

    public sealed class IdRequired()
        : DomainException("Class.IdRequired", "El identificador de la clase no puede estar vacío.");

    public sealed class CreatorRequired()
        : DomainException("Class.CreatorRequired", "La clase debe indicar quién la crea.");

    public sealed class SchoolYearRequired()
        : DomainException("Class.SchoolYearRequired", "La clase debe pertenecer a un curso escolar.");

    public sealed class NameRequired()
        : DomainException("Class.NameRequired", "El nombre de la clase no puede estar vacío.");
}
