using MediatR;
using SumiClas.Schools.Application;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Application;

namespace SumiClas.SchoolYears.Application;

public static class SchoolYearCreate
{
    public sealed record Command(Guid Id, Guid SchoolId, string Label, DateOnly StartDate, DateOnly EndDate)
        : IRequest, ICommand;

    public sealed class Create(ISchoolYearRepository repository, IQueryBus queryBus) : IUseCase
    {
        public async Task ExecuteAsync(Command command, CancellationToken ct = default)
        {
            await queryBus.Send(new SchoolEnsure.Query(command.SchoolId), ct);

            SchoolYear schoolYear = SchoolYear.Create(command.Id, command.SchoolId, command.Label, command.StartDate, command.EndDate);

            await repository.AddAsync(schoolYear, ct);
        }
    }

    public sealed class Handler(Create create) : IRequestHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct)
            => create.ExecuteAsync(command, ct);
    }
}
