namespace SumiClas.ClassExpenses.Domain;

public interface IClassExpenseRepository
{
    Task<ClassExpense?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(ClassExpense classExpense, CancellationToken ct = default);
}
