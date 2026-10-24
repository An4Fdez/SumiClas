using SumiClas.Shared.Domain;

namespace SumiClas.WeeklySummaries.Domain;

public static class WeeklySummaryExceptions
{
    public sealed class IdRequired()
        : DomainException("WeeklySummary.IdRequired", "El identificador del resumen no puede estar vacío.");

    public sealed class ClassRequired()
        : DomainException("WeeklySummary.ClassRequired", "El resumen debe indicar la clase.");

    public sealed class WeekStartInvalid()
        : DomainException("WeeklySummary.WeekStartInvalid", "La semana del resumen debe empezar en lunes.");

    public sealed class InputHashRequired()
        : DomainException("WeeklySummary.InputHashRequired", "El resumen debe indicar con qué datos se escribió.");

    public sealed class ContentRequired()
        : DomainException("WeeklySummary.ContentRequired", "El resumen no puede estar vacío.");
}
