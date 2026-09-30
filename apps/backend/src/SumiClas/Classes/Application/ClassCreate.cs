using MediatR;
using SumiClas.Classes.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.Classes.Application;

public static class ClassCreate
{
    public sealed record Command(Guid Id, Guid SchoolYearId, string Name) : IRequest<Result>, ICommand;

    public sealed class UseCase(IClassRepository classes, IQueryBus queryBus)
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var schoolYear = await queryBus.Send(new SchoolYearEnsureIsActive.Query(command.SchoolYearId), ct);
            if (schoolYear.IsFailure)
            {
                return schoolYear;
            }

            var creation = Class.Create(command.Id, command.SchoolYearId, command.Name);
            if (creation.IsFailure)
            {
                return Result.Failure(creation.Error!);
            }

            await classes.AddAsync(creation.Value, ct);

            return Result.Success();
        }
    }

    public sealed class Handler(UseCase useCase) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => useCase.ExecuteAsync(command, ct);
    }
}
