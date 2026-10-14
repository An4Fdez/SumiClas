namespace SumiClas.Tests.Shared.Application;

using SumiClas.Shared.Application;

public static class DuplicateKeyExceptionMother
{
    public static DuplicateKeyException Create(string? constraintName, Exception innerException)
        => new(constraintName, innerException);

    public static DuplicateKeyException With(string constraintName)
        => Create(constraintName, new InvalidOperationException(StringMother.Random("inner")));
}
