using MediatR;
using SumiClas.CalendarEvents.Application;
using SumiClas.ClassExpenses.Domain;
using SumiClas.Classes.Application;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.ClassExpenses.Application;

public static class ClassExpenseUpdate
{
    /// <summary>
    /// The author changes the concept, the amount and the related event. The participants and the payments already
    /// made stay as they are.
    /// </summary>
    public sealed record Command(
        Guid Id,
        string Item,
        decimal TotalAmount,
        Guid? RelatedEventId) : IRequest, ICommand;

    public sealed class Update(IClassExpenseRepository repository, IQueryBus queryBus, IClock clock, IUserProvider userProvider) : IUseCase
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

            ClassResponse schoolClass = await queryBus.Send(new ClassFinderById.Query(classExpense.ClassId), ct);

            await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.SchoolYearId), ct);

            if (command.RelatedEventId is { } relatedEventId)
            {
                await queryBus.Send(new CalendarEventFinderById.Query(relatedEventId), ct);
            }

            classExpense.Update(command.Item, command.TotalAmount, command.RelatedEventId, clock);
        }
    }

    public sealed class Handler(Update update) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => update.ExecuteAsync(command, ct);
    }
}
