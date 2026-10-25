namespace SumiClas.Tests.ClassExpenses.Application;

using SumiClas.ClassExpenses.Application;
using SumiClas.Tests.Shared;

public static class ClassExpenseOpenResponseMother
{
    public static ClassExpenseOpenResponse With(string? item = null)
        => new(item ?? StringMother.Random("expense"));
}
