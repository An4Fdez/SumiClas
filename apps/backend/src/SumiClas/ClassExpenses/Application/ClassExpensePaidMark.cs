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
    public sealed record Command(Guid ClassExpenseId, Guid GuardianId) : IRequest<Result>, ICommand;

    public sealed class Mark(IClassExpenseRepository repository, IQueryBus queryBus, IUserProvider userProvider) : IUseCase
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var classExpense = await repository.GetByIdAsync(command.ClassExpenseId, ct);
            if (classExpense is null)
            {
                return Result.Failure(ClassExpenseErrors.NotFound);
            }

            var schoolClass = await queryBus.Send(new ClassFinderById.Query(classExpense.ClassId), ct);
            if (schoolClass.IsFailure)
            {
                return Result.Failure(schoolClass.Error!);
            }

            var current = userProvider.GetCurrentUserId();
            if (current.IsFailure)
            {
                return Result.Failure(current.Error!);
            }

            var representative = await queryBus.Send(new UserClassRepresentativeEnsure.Query(current.Value, classExpense.ClassId), ct);
            if (representative.IsFailure)
            {
                return representative;
            }

            var schoolYear = await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.Value.SchoolYearId), ct);
            if (schoolYear.IsFailure)
            {
                return schoolYear;
            }

            return classExpense.MarkAsPaid(command.GuardianId);
        }
    }

    public sealed class Handler(Mark mark) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => mark.ExecuteAsync(command, ct);
    }
}
