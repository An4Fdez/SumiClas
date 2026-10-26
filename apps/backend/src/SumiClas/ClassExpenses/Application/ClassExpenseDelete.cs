using MediatR;
using SumiClas.ClassExpenses.Domain;
using SumiClas.Classes.Application;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;

namespace SumiClas.ClassExpenses.Application;

public static class ClassExpenseDelete
{
    /// <summary>The author deletes the expense for good, with its participants; once somebody has paid it cannot be deleted.</summary>
    public sealed record Command(Guid Id) : IRequest, ICommand;

    public sealed class Delete(IClassExpenseRepository repository, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            Guid current = userProvider.GetCurrentUserId();

            ClassExpense classExpense = await repository.GetByIdAsync(command.Id, ct)
                ?? throw new ClassExpenseExceptions.NotFound();

            if (classExpense.AuthorId != current)
            {
                throw new ClassExpenseExceptions.ChangeForbidden();
            }

            if (classExpense.HasPayments)
            {
                throw new ClassExpenseExceptions.HasPayments();
            }

            ClassResponse schoolClass = await queryBus.Send(new ClassFinderById.Query(classExpense.ClassId), ct);

            await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.SchoolYearId), ct);

            repository.Remove(classExpense);
        }
    }

    public sealed class Handler(Delete delete) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => delete.ExecuteAsync(command, ct);
    }
}
