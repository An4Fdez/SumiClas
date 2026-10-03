namespace SumiClas.ClassExpenses.Domain;

public interface IClassExpenseRepository
{
    Task AddAsync(ClassExpense classExpense, CancellationToken ct = default);
}
