namespace SumiClas.Shared.Domain;

public static class PaginationExceptions
{
    public sealed class InvalidCursor() : DomainException(
        "Pagination.InvalidCursor",
        "No se ha podido cargar esa página de resultados. Vuelve a cargar la lista.");

    public sealed class InvalidLimit()
        : DomainException("Pagination.InvalidLimit", "Se pueden pedir entre 1 y 50 resultados cada vez.");
}
