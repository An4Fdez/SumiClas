namespace SumiClas.Shared.Domain;

public static class PaginationErrors
{
    public static readonly Error InvalidCursor = new("Pagination.InvalidCursor", "No se ha podido cargar esa página de resultados. Vuelve a cargar la lista.");

    public static readonly Error InvalidLimit = new("Pagination.InvalidLimit", "Se pueden pedir entre 1 y 50 resultados cada vez.");
}
