namespace SumiClas.Shared.Domain;

public static class SharedExceptions
{
    public sealed class AlreadyExists()
        : DomainException(SharedErrors.AlreadyExists.Code, SharedErrors.AlreadyExists.Message);

    public sealed class ConcurrencyConflict()
        : DomainException(SharedErrors.ConcurrencyConflict.Code, SharedErrors.ConcurrencyConflict.Message);
}
