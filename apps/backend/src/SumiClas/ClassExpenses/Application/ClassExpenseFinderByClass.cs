using MediatR;
using SumiClas.CalendarEvents.Application;
using SumiClas.Classes.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;
using SumiClas.ClassExpenses.Domain;

namespace SumiClas.ClassExpenses.Application;

public static class ClassExpenseFinderByClass
{
    /// <summary>The expenses of the class with who participates and who has paid, newest first, one page at a time.</summary>
    public sealed record Query(Guid ClassId, int? Limit = null, string? Cursor = null) : IQuery<PagedResponse<ClassExpenseResponse>>;

    public sealed class Finder(IClassExpenseRepository repository, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
    {
        public async Task<PagedResponse<ClassExpenseResponse>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var pageRequest = Pagination.Resolve(query.Limit, query.Cursor);

            var page = Pagination.ByCreatedAt(pageRequest);

            await queryBus.Send(new ClassFinderById.Query(query.ClassId), ct);

            var current = userProvider.GetCurrentUserId();

            await queryBus.Send(new UserClassAccessEnsure.Query(current, query.ClassId), ct);

            var candidates = await repository.SearchAsync(new ClassExpenseSpecification(classId: query.ClassId, page: page), ct);
            var (expenses, nextCursor) = Pagination.Cut(
                candidates.OrderByDescending(expense => expense.CreatedAt).ThenByDescending(expense => expense.Id, Pagination.IdComparer).ToList(),
                pageRequest.Limit,
                expense => Pagination.CursorOf(expense.CreatedAt, expense.Id));

            var userIds = expenses
                .Select(expense => expense.AuthorId)
                .Concat(expenses.SelectMany(expense => expense.Participants.Select(participant => participant.GuardianId)))
                .Distinct()
                .ToList();
            var users = (await queryBus.Send(new UserClassMemberFinderByIds.Query(query.ClassId, userIds), ct))
                .ToDictionary(user => user.UserId);

            var eventIds = expenses.Where(expense => expense.RelatedEventId is not null)
                .Select(expense => expense.RelatedEventId!.Value).Distinct().ToList();
            var events = (await queryBus.Send(new CalendarEventFinderByIds.Query(eventIds), ct))
                .ToDictionary(calendarEvent => calendarEvent.Id);

            IReadOnlyCollection<ClassExpenseResponse> items = expenses
                .Select(expense => new ClassExpenseResponse(
                    expense.Id,
                    expense.ClassId,
                    expense.Item,
                    expense.TotalAmount,
                    expense.RelatedEventId is { } relatedEventId ? events.GetValueOrDefault(relatedEventId) : null,
                    expense.AuthorId,
                    users.GetValueOrDefault(expense.AuthorId)?.FullName ?? string.Empty,
                    users.GetValueOrDefault(expense.AuthorId)?.ClassRole,
                    expense.CreatedAt,
                    expense.Participants
                        .Select(participant => new ExpenseParticipantResponse(
                            participant.GuardianId,
                            users.GetValueOrDefault(participant.GuardianId)?.FullName ?? string.Empty,
                            participant.HasPaid))
                        .OrderBy(participant => participant.FullName)
                        .ToList()))
                .ToList();

            return new PagedResponse<ClassExpenseResponse>(items, nextCursor);
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, PagedResponse<ClassExpenseResponse>>
    {
        public Task<PagedResponse<ClassExpenseResponse>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
