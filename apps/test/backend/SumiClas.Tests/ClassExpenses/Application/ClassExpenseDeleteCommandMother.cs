namespace SumiClas.Tests.ClassExpenses.Application;

using SumiClas.ClassExpenses.Application;

public static class ClassExpenseDeleteCommandMother
{
    public static ClassExpenseDelete.Command WithClassExpenseId(Guid classExpenseId) => new(classExpenseId);
}
