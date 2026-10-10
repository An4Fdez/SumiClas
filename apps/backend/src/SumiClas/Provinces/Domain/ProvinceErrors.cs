using SumiClas.Shared.Domain;

namespace SumiClas.Provinces.Domain;

public static class ProvinceErrors
{
    public static readonly Error NotFound = new("Province.NotFound", "No se ha encontrado la provincia.");
    public static readonly Error IdRequired = new("Province.IdRequired", "El identificador de la provincia no puede estar vacío.");
    public static readonly Error NameRequired = new("Province.NameRequired", "El nombre de la provincia no puede estar vacío.");
}
