namespace SumiClas.Tests.Shared.Application;

using SumiClas.Shared.Application;

public static class ConcurrencyConflictExceptionMother
{
    public static ConcurrencyConflictException Random()
        => new(new InvalidOperationException(StringMother.Random("inner")));
}
