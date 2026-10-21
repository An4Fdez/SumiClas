using SumiClas.Shared.Domain;

namespace SumiClas.Localities.Domain;

public static class LocalityExceptions
{
    public sealed class IdRequired()
        : DomainException("Locality.IdRequired", "El identificador de la población no puede estar vacío.");

    public sealed class NameRequired()
        : DomainException("Locality.NameRequired", "El nombre de la población no puede estar vacío.");

    public sealed class ProvinceRequired()
        : DomainException("Locality.ProvinceRequired", "La población debe pertenecer a una provincia.");
}
