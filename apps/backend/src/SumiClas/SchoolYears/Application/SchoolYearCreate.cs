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

    public sealed class UseCase(ISchoolYearRepository schoolYears, IQueryBus queryBus)
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var school = await queryBus.Send(new SchoolFinderById.Query(command.SchoolId), ct);
            if (school.IsFailure)
            {
                return Result.Failure(school.Error!);
            }

            var creation = SchoolYear.Create(command.Id, command.SchoolId, command.Label, command.StartDate, command.EndDate);
            if (creation.IsFailure)
            {
                return Result.Failure(creation.Error!);
            }

            await schoolYears.AddAsync(creation.Value, ct);

            return Result.Success();
        }
    }

    public sealed class Handler(UseCase useCase) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => useCase.ExecuteAsync(command, ct);
    }
}
