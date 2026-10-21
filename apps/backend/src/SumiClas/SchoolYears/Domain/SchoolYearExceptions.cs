using SumiClas.Shared.Domain;

namespace SumiClas.SchoolYears.Domain;

public static class SchoolYearExceptions
{
    public sealed class InvalidStatus()
        : DomainException("SchoolYear.InvalidStatus", "El estado del curso escolar no es válido.");

    public sealed class NotFound()
        : DomainException("SchoolYear.NotFound", "No se ha encontrado el curso escolar.");

    public sealed class IdRequired()
        : DomainException("SchoolYear.IdRequired", "El identificador del curso escolar no puede estar vacío.");

    public sealed class SchoolRequired()
        : DomainException("SchoolYear.SchoolRequired", "El curso escolar debe pertenecer a un colegio.");

    public sealed class LabelRequired()
        : DomainException("SchoolYear.LabelRequired", "La etiqueta del curso escolar no puede estar vacía.");

    public sealed class InvalidDateRange()
        : DomainException(
            "SchoolYear.InvalidDateRange",
            "La fecha de fin del curso escolar debe ser posterior a la de inicio.");

    public sealed class Archived()
        : DomainException("SchoolYear.Archived", "Este curso escolar está archivado y ya no admite cambios.");
}
