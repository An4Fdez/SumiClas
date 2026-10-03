using SumiClas.ClassExpenses.Domain;
using SumiClas.Shared.Infrastructure.Persistence;

namespace SumiClas.ClassExpenses.Infrastructure;

public sealed class ClassExpenseRepository(SumiClasDbContext dbContext)
    : BaseRepository<ClassExpense>(dbContext), IClassExpenseRepository;
