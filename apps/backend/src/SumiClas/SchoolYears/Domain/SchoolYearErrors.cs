using SumiClas.Shared.Domain;

namespace SumiClas.SchoolYears.Domain;

public static class SchoolYearErrors
{
    public static readonly Error InvalidStatus = new("SchoolYear.InvalidStatus", "El estado del curso escolar no es válido.");
    public static readonly Error NotFound = new("SchoolYear.NotFound", "No se ha encontrado el curso escolar.");
    public static readonly Error IdRequired = new("SchoolYear.IdRequired", "El identificador del curso escolar no puede estar vacío.");
    public static readonly Error SchoolRequired = new("SchoolYear.SchoolRequired", "El curso escolar debe pertenecer a un colegio.");
    public static readonly Error LabelRequired = new("SchoolYear.LabelRequired", "La etiqueta del curso escolar no puede estar vacía.");
    public static readonly Error InvalidDateRange = new("SchoolYear.InvalidDateRange", "La fecha de fin del curso escolar debe ser posterior a la de inicio.");
    public static readonly Error Archived = new("SchoolYear.Archived", "Este curso escolar está archivado y ya no admite cambios.");
}
