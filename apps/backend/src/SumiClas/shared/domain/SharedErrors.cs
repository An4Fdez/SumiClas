namespace SumiClas.Shared.Domain;

public static class SharedErrors
{
    public static readonly Error AlreadyExists = new("Shared.AlreadyExists", "Ya existe un registro con ese identificador o con esos datos únicos.");
}
