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
    public sealed record Query(Guid ClassId, int? Limit = null, string? Cursor = null) : IQuery<Result<PagedResponse<ClassExpenseResponse>>>;

    public sealed class Finder(IClassExpenseRepository repository, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
    {
        public async Task<Result<PagedResponse<ClassExpenseResponse>>> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            var pageRequest = Pagination.Resolve(query.Limit, query.Cursor);
            if (pageRequest.IsFailure)
            {
                return Result<PagedResponse<ClassExpenseResponse>>.Failure(pageRequest.Error!);
            }

            var page = Pagination.ByCreatedAt(pageRequest.Value);
            if (page.IsFailure)
            {
                return Result<PagedResponse<ClassExpenseResponse>>.Failure(page.Error!);
            }

            var schoolClass = await queryBus.Send(new ClassFinderById.Query(query.ClassId), ct);
            if (schoolClass.IsFailure)
            {
                return Result<PagedResponse<ClassExpenseResponse>>.Failure(schoolClass.Error!);
            }

            var current = userProvider.GetCurrentUserId();
            if (current.IsFailure)
            {
                return Result<PagedResponse<ClassExpenseResponse>>.Failure(current.Error!);
            }

            var access = await queryBus.Send(new UserClassAccessEnsure.Query(current.Value, query.ClassId), ct);
            if (access.IsFailure)
            {
                return Result<PagedResponse<ClassExpenseResponse>>.Failure(access.Error!);
            }

            var candidates = await repository.SearchAsync(new ClassExpenseSpecification(classId: query.ClassId, page: page.Value), ct);
            var (expenses, nextCursor) = Pagination.Cut(
                candidates.OrderByDescending(expense => expense.CreatedAt).ThenByDescending(expense => expense.Id, Pagination.IdComparer).ToList(),
                pageRequest.Value.Limit,
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

            return Result<PagedResponse<ClassExpenseResponse>>.Success(new PagedResponse<ClassExpenseResponse>(items, nextCursor));
        }
    }

    public sealed class Handler(Finder finder) : IRequestHandler<Query, Result<PagedResponse<ClassExpenseResponse>>>
    {
        public Task<Result<PagedResponse<ClassExpenseResponse>>> Handle(Query query, CancellationToken ct)
            => finder.ExecuteAsync(query, ct);
    }
}
