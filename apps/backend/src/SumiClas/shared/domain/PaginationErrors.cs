namespace SumiClas.Shared.Domain;

public static class PaginationErrors
{
    public static readonly Error InvalidCursor = new("Pagination.InvalidCursor", "El cursor de paginación no es válido.");

    public static readonly Error InvalidLimit = new("Pagination.InvalidLimit", "El límite de paginación debe estar entre 1 y 50.");
}
