using MediatR;
using SumiClas.Classes.Application;
using SumiClas.ClassExpenses.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.ClassExpenses.Application;

public static class ClassExpenseOpenFinderByClass
{
    /// <summary>
    /// What the expenses of the class that still have a participant who has not paid are for, oldest first. It names
    /// nobody and does not depend on who asks.
    /// </summary>
    public sealed record Query(Guid ClassId) : IQuery<IReadOnlyCollection<ClassExpenseOpenResponse>>;

    public sealed class Finder(IClassExpenseRepository repository, IQueryBus queryBus, IUserProvider userProvider)
        : IUseCase
    {
        public async Task<IReadOnlyCollection<ClassExpenseOpenResponse>> ExecuteAsync(
            Query query,
            CancellationToken ct = default)
        {
            Guid current = userProvider.GetCurrentUserId();

            await queryBus.Send(new ClassFinderById.Query(query.ClassId), ct);

            await queryBus.Send(new UserClassAccessEnsure.Query(current, query.ClassId), ct);

            return (await repository.SearchAsync(
                    new ClassExpenseSpecification(classId: query.ClassId, participantHasPaid: false), ct))
                .OrderBy(expense => expense.CreatedAt)
                .ThenBy(expense => expense.Id, Pagination.IdComparer)
                .Select(expense => new ClassExpenseOpenResponse(expense.Item))
                .ToList();
        }
    }

    public sealed class Handler(Finder finder)
        : IRequestHandler<Query, IReadOnlyCollection<ClassExpenseOpenResponse>>
    {
        public Task<IReadOnlyCollection<ClassExpenseOpenResponse>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
