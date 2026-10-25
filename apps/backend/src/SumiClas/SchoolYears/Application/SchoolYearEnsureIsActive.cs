using MediatR;
using SumiClas.SchoolYears.Domain;
using SumiClas.SchoolYears.Domain.Services;
using SumiClas.Shared.Application;

namespace SumiClas.SchoolYears.Application;

public static class SchoolYearEnsureIsActive
{
    public sealed record Query(Guid SchoolYearId) : IQuery<Unit>;

    public sealed class EnsureIsActive(SchoolYearFinder finder) : IUseCase
    {
        public async Task<Unit> ExecuteAsync(Query query, CancellationToken ct = default)
        {
            SchoolYear schoolYear = await finder.FindAsync(query.SchoolYearId, ct);
            schoolYear.EnsureIsActive();
            return Unit.Value;
        }
    }

    public sealed class Handler(EnsureIsActive ensureIsActive) : IRequestHandler<Query, Unit>
    {
        public Task<Unit> Handle(Query query, CancellationToken ct)
            => ensureIsActive.ExecuteAsync(query, ct);
    }
}
