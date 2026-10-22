namespace SumiClas.Shared.Domain;

public static class SharedExceptions
{
    public sealed class AlreadyExists()
        : DomainException("Shared.AlreadyExists", "Ya existe un registro con esos datos.");

    public sealed class ConcurrencyConflict() : DomainException(
        "Shared.ConcurrencyConflict",
        "Otra petición modificó lo mismo a la vez. Vuelve a intentarlo.");
}
