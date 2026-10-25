using MediatR;
using SumiClas.Assignments.Domain;
using SumiClas.Classes.Application;
using SumiClas.Shared.Application;
using SumiClas.Users.Application;

namespace SumiClas.Assignments.Application;

public static class AssignmentPeriodFinderByClass
{
    /// <summary>
    /// The homework of the class, official or proposed, due from the day <c>From</c> to the day <c>To</c>, both
    /// included, earliest first. It carries no author, comment or completion, so the answer does not depend on who
    /// asks. A period that ends before it starts has none.
    /// </summary>
    public sealed record Query(Guid ClassId, DateOnly From, DateOnly To)
        : IQuery<IReadOnlyCollection<AssignmentPeriodResponse>>;

    public sealed class Finder(IAssignmentRepository repository, IQueryBus queryBus, IUserProvider userProvider)
        : IUseCase
    {
        public async Task<IReadOnlyCollection<AssignmentPeriodResponse>> ExecuteAsync(
            Query query,
            CancellationToken ct = default)
        {
            Guid current = userProvider.GetCurrentUserId();

            await queryBus.Send(new ClassFinderById.Query(query.ClassId), ct);

            await queryBus.Send(new UserClassAccessEnsure.Query(current, query.ClassId), ct);

            return (await repository.SearchAsync(
                    new AssignmentSpecification(classId: query.ClassId, dueFrom: query.From, dueTo: query.To), ct))
                .OrderBy(assignment => assignment.DueDate)
                .ThenBy(assignment => assignment.Title, StringComparer.Ordinal)
                .Select(assignment => new AssignmentPeriodResponse(assignment.Title, assignment.DueDate))
                .ToList();
        }
    }

    public sealed class Handler(Finder finder)
        : IRequestHandler<Query, IReadOnlyCollection<AssignmentPeriodResponse>>
    {
        public Task<IReadOnlyCollection<AssignmentPeriodResponse>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
