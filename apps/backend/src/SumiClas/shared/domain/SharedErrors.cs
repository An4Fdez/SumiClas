namespace SumiClas.Shared.Domain;

public static class SharedErrors
{
    public static readonly Error AlreadyExists = new("Shared.AlreadyExists", "Ya existe un registro con esos datos.");
    public static readonly Error ConcurrencyConflict = new("Shared.ConcurrencyConflict", "Otra petición modificó lo mismo a la vez. Vuelve a intentarlo.");
}
