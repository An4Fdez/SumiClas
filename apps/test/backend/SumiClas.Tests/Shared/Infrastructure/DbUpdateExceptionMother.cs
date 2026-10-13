namespace SumiClas.Tests.Shared.Infrastructure;

using Microsoft.EntityFrameworkCore;

public static class DbUpdateExceptionMother
{
    public static DbUpdateException Create(string message, Exception? innerException)
        => innerException is null ? new DbUpdateException(message) : new DbUpdateException(message, innerException);

    public static DbUpdateException With(Exception innerException)
        => Create(StringMother.Random("message"), innerException);

    public static DbUpdateException WithoutInner() => Create(StringMother.Random("message"), null);

    public static DbUpdateConcurrencyException Concurrency(Exception innerException)
        => new(StringMother.Random("message"), innerException);
}
