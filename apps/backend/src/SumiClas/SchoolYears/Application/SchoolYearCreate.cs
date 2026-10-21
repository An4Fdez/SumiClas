using MediatR;
using SumiClas.Schools.Application;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.SchoolYears.Application;

public static class SchoolYearCreate
{
    public sealed record Command(Guid Id, Guid SchoolId, string Label, DateOnly StartDate, DateOnly EndDate)
        : IRequest<Result>, ICommand;

    public sealed class Create(ISchoolYearRepository repository, IQueryBus queryBus) : IUseCase
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            await queryBus.Send(new SchoolEnsure.Query(command.SchoolId), ct);

            var creation = SchoolYear.Create(command.Id, command.SchoolId, command.Label, command.StartDate, command.EndDate);
            if (creation.IsFailure)
            {
                return Result.Failure(creation.Error!);
            }

            await repository.AddAsync(creation.Value, ct);

            return Result.Success();
        }
    }

    public sealed class Handler(Create create) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => create.ExecuteAsync(command, ct);
    }
}
