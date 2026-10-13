namespace SumiClas.Tests.Shared.Domain;

using SumiClas.Shared.Domain;

public static class ErrorMother
{
    public static Error Random() => With();

    public static Error With(string? code = null, string? message = null)
        => new(code ?? "Test." + StringMother.Random("code"), message ?? StringMother.Random("message"));
}
