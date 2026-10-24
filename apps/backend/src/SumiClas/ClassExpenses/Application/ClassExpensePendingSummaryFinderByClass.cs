using MediatR;
using SumiClas.Classes.Application;
using SumiClas.ClassExpenses.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.ClassExpenses.Application;

public static class ClassExpensePendingSummaryFinderByClass
{
    /// <summary>
    /// Whether the current user owes a payment in the class, and what the oldest expense they have not paid is for.
    /// </summary>
    public sealed record Query(Guid ClassId) : IQuery<ClassExpensePendingSummaryResponse>;

    public sealed class Finder(IClassExpenseRepository repository, IQueryBus queryBus, IUserProvider userProvider)
        : IUseCase
    {
        public async Task<ClassExpensePendingSummaryResponse> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            Guid current = userProvider.GetCurrentUserId();

            await queryBus.Send(new ClassFinderById.Query(query.ClassId), ct);

            await queryBus.Send(new UserClassAccessEnsure.Query(current, query.ClassId), ct);

            IReadOnlyCollection<ClassExpense> pending = await repository.SearchAsync(
                new ClassExpenseSpecification(classId: query.ClassId, participantId: current, participantHasPaid: false), ct);

            ClassExpense? oldest = pending
                .OrderBy(expense => expense.CreatedAt)
                .ThenBy(expense => expense.Id, Pagination.IdComparer)
                .FirstOrDefault();

            return new ClassExpensePendingSummaryResponse(oldest is not null, oldest?.Item);
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, ClassExpensePendingSummaryResponse>
    {
        public Task<ClassExpensePendingSummaryResponse> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
