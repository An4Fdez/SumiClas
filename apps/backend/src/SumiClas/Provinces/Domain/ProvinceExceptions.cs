using SumiClas.Shared.Domain;

namespace SumiClas.Provinces.Domain;

public static class ProvinceExceptions
{
    public sealed class IdRequired()
        : DomainException("Province.IdRequired", "El identificador de la provincia no puede estar vacío.");

    public sealed class NameRequired()
        : DomainException("Province.NameRequired", "El nombre de la provincia no puede estar vacío.");
}
