using MediatR;
using SumiClas.Classes.Domain;
using SumiClas.SchoolYears.Application;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.Classes.Application;

public static class ClassRename
{
    public sealed record Command(Guid ClassId, string Name) : IRequest<Result>, ICommand;

    public sealed class UseCase(IClassRepository classes, IQueryBus queryBus)
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var schoolClass = await classes.GetByIdAsync(command.ClassId, ct);
            if (schoolClass is null)
            {
                return Result.Failure(ClassErrors.NotFound);
            }

            var schoolYear = await queryBus.Send(new SchoolYearEnsureIsActive.Query(schoolClass.SchoolYearId), ct);
            if (schoolYear.IsFailure)
            {
                return schoolYear;
            }

            return schoolClass.Rename(command.Name);
        }
    }

    public sealed class Handler(UseCase useCase) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => useCase.ExecuteAsync(command, ct);
    }
}
