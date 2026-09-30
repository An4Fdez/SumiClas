using SumiClas.Shared.Domain;

namespace SumiClas.Schools.Domain;

public static class SchoolErrors
{
    public static readonly Error NotFound = new("School.NotFound", "No se ha encontrado el colegio.");
    public static readonly Error IdRequired = new("School.IdRequired", "El identificador del colegio no puede estar vacío.");
    public static readonly Error NameRequired = new("School.NameRequired", "El nombre del colegio no puede estar vacío.");
    public static readonly Error OfficialCodeRequired = new("School.OfficialCodeRequired", "El código oficial del colegio no puede estar vacío.");
}
