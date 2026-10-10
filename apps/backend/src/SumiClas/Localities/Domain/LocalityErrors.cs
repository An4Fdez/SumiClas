using SumiClas.Shared.Domain;

namespace SumiClas.Localities.Domain;

public static class LocalityErrors
{
    public static readonly Error NotFound = new("Locality.NotFound", "No se ha encontrado la población.");
    public static readonly Error IdRequired = new("Locality.IdRequired", "El identificador de la población no puede estar vacío.");
    public static readonly Error NameRequired = new("Locality.NameRequired", "El nombre de la población no puede estar vacío.");
    public static readonly Error ProvinceRequired = new("Locality.ProvinceRequired", "La población debe pertenecer a una provincia.");
}
