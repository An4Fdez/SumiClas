namespace SumiClas.Tests.ClassExpenses.Application;

using SumiClas.ClassExpenses.Application;

public static class ClassExpensePaidMarkCommandMother
{
    public static ClassExpensePaidMark.Command With(Guid classExpenseId, Guid guardianId)
        => new(classExpenseId, guardianId);
}
