using SumiClas.Shared.Domain;

namespace SumiClas.Classes.Domain;

public static class ClassErrors
{
    public static readonly Error IdRequired = new("Class.IdRequired", "El identificador de la clase no puede estar vacío.");
    public static readonly Error SchoolYearRequired = new("Class.SchoolYearRequired", "La clase debe pertenecer a un curso escolar.");
    public static readonly Error NameRequired = new("Class.NameRequired", "El nombre de la clase no puede estar vacío.");
}
