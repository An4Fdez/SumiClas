using MediatR;
using SumiClas.Classes.Application;
using SumiClas.ClassExpenses.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;
using SumiClas.Users.Application;

namespace SumiClas.ClassExpenses.Application;

public static class ClassExpensePaidMark
{
    public sealed record Command(Guid ClassExpenseId, Guid GuardianId) : IRequest, ICommand;

    public sealed class Mark(IClassExpenseRepository repository, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var classExpense = await repository.GetByIdAsync(command.ClassExpenseId, ct)
                ?? throw new ClassExpenseExceptions.NotFound();

            var schoolClass = await queryBus.Send(new ClassFinderById.Query(classExpense.ClassId), ct);

            var current = userProvider.GetCurrentUserId();

            // A family marks its own share; marking someone else's is for the class delegate.
            if (current == command.GuardianId)
            {
                await queryBus.Send(new UserClassAccessEnsure.Query(current, classExpense.ClassId), ct);
            }
            else
            {
                await queryBus.Send(new UserClassRepresentativeEnsure.Query(current, classExpense.ClassId), ct);
            }

            await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.SchoolYearId), ct);

            classExpense.MarkAsPaid(command.GuardianId);
        }
    }

    public sealed class Handler(Mark mark) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => mark.ExecuteAsync(command, ct);
    }
}
