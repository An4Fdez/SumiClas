using SumiClas.Shared.Domain;

namespace SumiClas.Schools.Domain;

public static class SchoolExceptions
{
    public sealed class NotFound() : DomainException("School.NotFound", "No se ha encontrado el colegio.");

    public sealed class IdRequired()
        : DomainException("School.IdRequired", "El identificador del colegio no puede estar vacío.");

    public sealed class NameRequired()
        : DomainException("School.NameRequired", "El nombre del colegio no puede estar vacío.");

    public sealed class OfficialCodeRequired()
        : DomainException("School.OfficialCodeRequired", "El código oficial del colegio no puede estar vacío.");

    public sealed class LocalityRequired()
        : DomainException("School.LocalityRequired", "El colegio debe pertenecer a una población.");

    public sealed class PostalCodeInvalid()
        : DomainException("School.PostalCodeInvalid", "El código postal del colegio no es válido.");

    public sealed class LocalityFilterRequired()
        : DomainException("School.LocalityFilterRequired", "Hay que indicar la población para buscar colegios.");
}
