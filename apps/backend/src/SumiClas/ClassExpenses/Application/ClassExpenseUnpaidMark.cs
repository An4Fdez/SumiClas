using MediatR;
using SumiClas.Classes.Application;
using SumiClas.ClassExpenses.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.ClassExpenses.Application;

public static class ClassExpenseUnpaidMark
{
    public sealed record Command(Guid ClassExpenseId, Guid GuardianId) : IRequest<Result>, ICommand;

    public sealed class Mark(IClassExpenseRepository repository, IQueryBus queryBus) : IUseCase
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

            var schoolYear = await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.Value.SchoolYearId), ct);
            if (schoolYear.IsFailure)
            {
                return schoolYear;
            }

            return classExpense.MarkAsUnpaid(command.GuardianId);
        }
    }

    public sealed class Handler(Mark mark) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => mark.ExecuteAsync(command, ct);
    }
}
