namespace SumiClas.Tests.ClassExpenses.Application;

using SumiClas.ClassExpenses.Application;

public static class ClassExpenseUnpaidMarkCommandMother
{
    public static ClassExpenseUnpaidMark.Command With(Guid classExpenseId, Guid guardianId)
        => new(classExpenseId, guardianId);
}
