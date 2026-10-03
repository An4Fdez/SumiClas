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
    /// <summary>The expenses of the class with who participates and who has paid, newest first.</summary>
    public sealed record Query(Guid ClassId) : IQuery<Result<IReadOnlyCollection<ClassExpenseResponse>>>;

    public sealed class Handler(IClassExpenseRepository classExpenses, IQueryBus queryBus)
        : IRequestHandler<Query, Result<IReadOnlyCollection<ClassExpenseResponse>>>
    {
        public async Task<Result<IReadOnlyCollection<ClassExpenseResponse>>> Handle(Query query, CancellationToken ct)
        {
            var schoolClass = await queryBus.Send(new ClassFinderById.Query(query.ClassId), ct);
            if (schoolClass.IsFailure)
            {
                return Result<IReadOnlyCollection<ClassExpenseResponse>>.Failure(schoolClass.Error!);
            }

            var expenses = await classExpenses.SearchAsync(new ClassExpenseSpecification(classId: query.ClassId), ct);

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

            IReadOnlyCollection<ClassExpenseResponse> response = expenses
                .OrderByDescending(expense => expense.CreatedAt)
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

            return Result<IReadOnlyCollection<ClassExpenseResponse>>.Success(response);
        }
    }
}
