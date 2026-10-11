using MediatR;
using SumiClas.SchoolYears.Domain;
using SumiClas.Shared.Application;
using SumiClas.Shared.Domain;

namespace SumiClas.SchoolYears.Application;

public static class SchoolYearArchive
{
    public sealed record Command(Guid SchoolYearId) : IRequest<Result>, ICommand;

    public sealed class Archive(ISchoolYearRepository schoolYears, IClock clock) : IUseCase
    {
        public async Task<Result> ExecuteAsync(Command command, CancellationToken ct = default)
        {
            var schoolYear = await schoolYears.GetByIdAsync(command.SchoolYearId, ct);
            if (schoolYear is null)
            {
                return Result.Failure(SchoolYearErrors.NotFound);
            }

            return schoolYear.Archive(clock);
        }
    }

    public sealed class Handler(Archive archive) : IRequestHandler<Command, Result>
    {
        public Task<Result> Handle(Command command, CancellationToken ct)
            => archive.ExecuteAsync(command, ct);
    }
}
